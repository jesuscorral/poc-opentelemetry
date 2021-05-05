using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POC.OpenTelemetry.Worker.Helpers;
using System.Diagnostics;

namespace POC.OpenTelemetry.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddSingleton<MessageReceiver>()
                        .AddHostedService<Worker>()
                        .AddSingleton<GrpcClient>()
                        .AddCustomOpenTelemetry();
                });
    }
}
