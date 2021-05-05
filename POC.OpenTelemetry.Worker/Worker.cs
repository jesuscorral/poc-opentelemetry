using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace POC.OpenTelemetry.Worker
{
    public class Worker : BackgroundService
    {
        private readonly MessageReceiver _messageReceiver;

        public Worker(MessageReceiver messageReceiver,
            GrpcClient grpcClient)
        {
            _messageReceiver = messageReceiver ?? throw new ArgumentNullException(nameof(messageReceiver));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            _messageReceiver.StartConsumer();

            await Task.CompletedTask;
        }

    }
}
