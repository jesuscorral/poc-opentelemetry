using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;

namespace EventBusRabbitMQ
{
    public static class RabbitMqHelper
    {
        public const string QueueName = "poc-telemetry-api-queue";
        public const string DefaultExchangeName = "";
        private static readonly ConnectionFactory _connectionFactory;

        static RabbitMqHelper()
        {
            _connectionFactory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS") ?? "guest",
                Port = 5672,
                VirtualHost = ConnectionFactory.DefaultVHost,
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(3000),
            };
        }

        public static IConnection CreateConnection()
        {
            return _connectionFactory.CreateConnection();
        }

        public static IModel CreateModelAndDeclareQueue(IConnection connection)
        {
            var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            return channel;
        }

        public static void StartConsumer(IModel channel, Action<BasicDeliverEventArgs> processMessage)
        {
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (bc, ea) => processMessage(ea);

            channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);
        }

        // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
        // See:
        //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#messaging-attributes
        //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#rabbitmq
        public static void AddMessagingTags(Activity activity)
        {
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.destination", DefaultExchangeName);
            activity?.SetTag("messaging.rabbitmq.routing_key", QueueName);
        }
    }
}
