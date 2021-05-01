using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventBusRabbitMQ
{
    public class EventBusRabbitMQService : IEventBusRabbitMQService
    {
        private readonly RabbitMqConfiguration _rabbitMqConfiguration;
        private IConnection _connection;
        private IModel _channel;

        public EventBusRabbitMQService(RabbitMqConfiguration rabbitMqConfiguration)
        {
            _rabbitMqConfiguration = rabbitMqConfiguration ?? throw new ArgumentNullException(nameof(rabbitMqConfiguration));

            Initialize();
        }

        public async Task Publish(IntegrationEvent @event)
        {
            if (ConnectionExists())
            {
                var json = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(exchange: "", routingKey: _rabbitMqConfiguration.QueueName, basicProperties: null, body: body);
            }
        }

        public void CloseConnection()
        {
            _connection?.Close();
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

            Initialize();

            return _connection != null;
        }

        private void Initialize()
        {
            CreateConnection();
            CreateQueue();
        }

        private void CreateQueue()
        {
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _rabbitMqConfiguration.QueueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);
        }
    }
}
