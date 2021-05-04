using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace POC.OpenTelemetry.Grpc.Helpers
{
    public static class StartupHelper
    {
        public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services)
        {

            return services.AddOpenTelemetryTracing((builder) => {
                builder.AddAspNetCoreInstrumentation(opt => opt.RecordException = true )
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("POC.OpenTelemetry.Grpc"))
                    .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                    .AddHttpClientInstrumentation()
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
