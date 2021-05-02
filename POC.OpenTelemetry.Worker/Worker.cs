using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using POC.OpenTelemetry.Grpc;
using POC.OpenTelemetry.Worker.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace POC.OpenTelemetry.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private const string QueueName = "poc-telemetry-api-queue";

        public Worker(IConfiguration configuration, ILogger<Worker> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var rabbitHostName = Environment.GetEnvironmentVariable("RABBIT_HOSTNAME");
            _connectionFactory = new ConnectionFactory
            {
                HostName = rabbitHostName ?? "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            };
            _connection = _connectionFactory.CreateConnection();

            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "poc-telemetry-api-queue",
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

            _logger.LogInformation($"Queue [{QueueName}] is waiting for messages.");

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                stoppingToken.ThrowIfCancellationRequested();

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (bc, ea) =>
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogInformation($"Processing msg: '{message}'.");
                    try
                    {
                        var order = JsonSerializer.Deserialize<UserAddedEvent>(message);
                        _logger.LogInformation($"Sending order #{order.Username} confirmation email to [{order.Username}].");


                        AppContext.SetSwitch(
               "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",
               true);

                        var host = _configuration["GrpcServiceHost"];

                        _logger.LogInformation($"Host client: {host}");

                        var channel = GrpcChannel.ForAddress(host);

                        var client = new Greeter.GreeterClient(channel);
                        await client.SayHelloAsync(new HelloRequest { Name = order.Username });

                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (JsonException)
                    {
                        _logger.LogError($"JSON Parse Error: '{message}'.");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                    catch (AlreadyClosedException)
                    {
                        _logger.LogInformation("RabbitMQ is closed!");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(default, e, e.Message);
                    }
                };

                _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

                await Task.CompletedTask;

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
