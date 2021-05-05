using EventBusRabbitMQ;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace POC.OpenTelemetry.API
{
    public class MessageSender : IDisposable
    {
        private static readonly ActivitySource _activitySource = new ActivitySource(nameof(MessageSender));
        private static readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;

        private readonly ILogger<MessageSender> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageSender(ILogger<MessageSender> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connection = RabbitMqHelper.CreateConnection();
            _channel = RabbitMqHelper.CreateModelAndDeclareQueue(_connection);
        }

        public string SendMessage(IntegrationEvent @event)
        {
            try
            {
                // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
                // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
                var activityName = $"{RabbitMqHelper.QueueName} send";

                using var activity = _activitySource.StartActivity(activityName, ActivityKind.Producer);
      

                // Create properties where to inject 
                var properties = _channel.CreateBasicProperties();

                // Depending on Sampling (and whether a listener is registered or not), the
                // activity above may not be created.
                // If it is created, then propagate its context.
                // If it is not created, the propagate the Current context,
                // if any.
                ActivityContext contextToInject = default;
                if (activity != null)
                {
                    contextToInject = activity.Context;
                }
                else if (Activity.Current != null)
                {
                    contextToInject = Activity.Current.Context;
                }
                // Inject the ActivityContext into the message headers to propagate trace context to the receiving service.
                _propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), properties, InjectTraceContextIntoBasicProperties);

                // The OpenTelemetry messaging specification defines a number of attributes. These attributes are added here.
                RabbitMqHelper.AddMessagingTags(activity);

                var json = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(json);

                //// Checks delay before sent messages in traces
                //Task.Delay(5000);

                _channel.BasicPublish(
                    exchange: RabbitMqHelper.DefaultExchangeName,
                    routingKey: RabbitMqHelper.QueueName,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation($"Message sent: [{body}]");

                return "Sent";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message publishing failed.");
                throw;
            }
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }

        private void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
        {
            try
            {
                if (props.Headers == null)
                {
                    props.Headers = new Dictionary<string, object>();
                }

                props.Headers[key] = value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to inject trace context.");
            }
        }
    }
}
