using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace POC.OpenTelemetry.Worker.Helpers
{
    public static class StartupHelpers
    {
        public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services)
        {
            return services.AddOpenTelemetryTracing((builder) => {
                builder.AddAspNetCoreInstrumentation(opt =>
                {
                    opt.RecordException = true;
                })
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("POC.OpenTelemetry.Worker"))
                    .AddSource(nameof(MessageReceiver))
                    .AddSource(nameof(GrpcClient))
                    .AddGrpcCoreInstrumentation()
                    .AddConsoleExporter()
                     .AddZipkinExporter(options =>
                     {
                         var zipkinHostName = Environment.GetEnvironmentVariable("ZIPKIN_HOSTNAME") ?? "localhost";
                         options.Endpoint = new Uri($"http://{zipkinHostName}:9411/api/v2/spans");
                     })
                    .AddJaegerExporter(jaegerOptions =>
                    {
                        jaegerOptions.AgentHost = "jaeger";
                        jaegerOptions.AgentPort = 6831;
                    });
            });
        }
    }
}