using Microsoft.Extensions.Hosting;
using OpenTelemetry.Context.Propagation;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace POC.OpenTelemetry.Worker
{
    public class Worker : BackgroundService
    {
        private readonly MessageReceiver _messageReceiver;

        private static readonly ActivitySource ActivitySource = new ActivitySource(nameof(Worker));
        private static readonly TextMapPropagator _propagator = new TraceContextPropagator();

        private const string QueueName = "poc-telemetry-api-queue";

        public Worker(MessageReceiver messageReceiver,
            GrpcClient grpcClient)
        {
            
            _messageReceiver = messageReceiver ?? throw new ArgumentNullException(nameof(messageReceiver));
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            _messageReceiver.StartConsumer();

            await Task.CompletedTask;

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            //    //stoppingToken.ThrowIfCancellationRequested();

            //    var consumer = new AsyncEventingBasicConsumer(_channel);
            //    consumer.Received += async (bc, ea) =>
            //    {

            //        try
            //        {


            //            // OpenTelemetry
            //            var parentContext = _propagator.Extract(default, ea.BasicProperties, this.ExtractTraceContextFromBasicProperties);
            //            Baggage.Current = parentContext.Baggage;

            //            var activityName = $"{ea.RoutingKey} receive";

            //            using (var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext))
            //            {
            //                try
            //                {
            //                    var message2 = Encoding.UTF8.GetString(ea.Body.Span.ToArray());

            //                    _logger.LogInformation($"Message received: [{message2}]");

            //                    activity?.SetTag("message", message2);



            //                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            //                    _logger.LogInformation($"Processing msg: '{body}'.");

            //                    // The OpenTelemetry messaging specification defines a number of attributes. These attributes are added here.
            //                    //RabbitMqHelper.AddMessagingTags(activity);
            //                    activity.SetTag("message", body);
            //                    activity.SetTag("message.Type", "queue");
            //                    activity.SetTag("message.System", "rabbitmq");
            //                    //activity.SetTag("message.path", )

            //                     var user = JsonSerializer.Deserialize<UserAddedEvent>(body);
            //                    _logger.LogInformation($"Sending order #{user.Username}");

            //                    _grpcClient.SendUser(user);
            //                }
            //                catch (Exception ex)
            //                {
            //                   _logger.LogError(ex, "Message processing failed.");
            //                }
            //            }



            //            _channel.BasicAck(ea.DeliveryTag, false);
            //        }
            //        catch (JsonException)
            //        {
            //            //_logger.LogError($"JSON Parse Error: '{message}'.");
            //            _channel.BasicNack(ea.DeliveryTag, false, false);
            //        }
            //        catch (AlreadyClosedException)
            //        {
            //            _logger.LogInformation("RabbitMQ is closed!");
            //        }
            //        catch (Exception e)
            //        {
            //            _logger.LogError(default, e, e.Message);
            //        }
            //    };

            //    _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            //    await Task.CompletedTask;

            //    await Task.Delay(1000, stoppingToken);
            //}
        }

    }
}
