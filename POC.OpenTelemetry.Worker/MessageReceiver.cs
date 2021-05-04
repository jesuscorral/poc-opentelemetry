using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using EventBusRabbitMQ;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using POC.OpenTelemetry.Worker.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace POC.OpenTelemetry.Worker
{
    public class MessageReceiver : IDisposable
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource(nameof(MessageReceiver));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private readonly ILogger<MessageReceiver> _logger;
        private readonly GrpcClient _grpcClient;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageReceiver(ILogger<MessageReceiver> logger, GrpcClient grpcClient)
        {
            _logger = logger;
            _grpcClient = grpcClient;
            _connection = RabbitMqHelper.CreateConnection();
            _channel = RabbitMqHelper.CreateModelAndDeclareQueue(_connection);
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }

        public void StartConsumer()
        {
            RabbitMqHelper.StartConsumer(_channel, ReceiveMessage);
        }

        public void ReceiveMessage(BasicDeliverEventArgs ea)
        {
            // Extract the PropagationContext of the upstream parent from the message headers.
            var parentContext = Propagator.Extract(default, ea.BasicProperties, this.ExtractTraceContextFromBasicProperties);
            Baggage.Current = parentContext.Baggage;

            // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
            // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
            var activityName = $"{ea.RoutingKey} receive";

            using (var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext))
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.Span.ToArray());

                    _logger.LogInformation($"Message received: [{message}]");

                    activity?.SetTag("message", message);

                    var user = JsonSerializer.Deserialize<UserAddedEvent>(message);
                    _grpcClient.SendUser(user);

                    // The OpenTelemetry messaging specification defines a number of attributes. These attributes are added here.
                    RabbitMqHelper.AddMessagingTags(activity);

                    // Simulate some work
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message processing failed.");
                }
            }
        }

        private IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
        {
            try
            {
                if (props.Headers.TryGetValue(key, out var value))
                {
                    var bytes = value as byte[];

                    

                    return new[] { Encoding.UTF8.GetString(bytes) };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract trace context: {ex}");
            }

            return Enumerable.Empty<string>();
        }
    }
}
