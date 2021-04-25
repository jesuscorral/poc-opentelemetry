using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace EventBusRabbitMQ
{
    public class EventBusRabbitMQService: IEventBusRabbitMQService
    {
        private readonly RabbitMqConfiguration _rabbitMqConfiguration;
        private IConnection _connection;

        public EventBusRabbitMQService(RabbitMqConfiguration rabbitMqConfiguration)
        {
            _rabbitMqConfiguration = rabbitMqConfiguration ?? throw new ArgumentNullException(nameof(rabbitMqConfiguration));

            CreateConnection();
        }

        public void Publish(IntegrationEvent @event)
        {
            if (ConnectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.QueueDeclare(queue: _rabbitMqConfiguration.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var json = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(json);

                    channel.BasicPublish(exchange: "", routingKey: _rabbitMqConfiguration.QueueName, basicProperties: null, body: body);
                }
            }

        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Port = _rabbitMqConfiguration.Port,
                    VirtualHost = ConnectionFactory.DefaultVHost,
                    HostName = _rabbitMqConfiguration.Hostname,
                    UserName = _rabbitMqConfiguration.UserName,
                    Password = _rabbitMqConfiguration.Password
                };
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create connection: {ex.Message}");
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }

            CreateConnection();

            return _connection != null;
        }
    }
}
