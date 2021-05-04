using EventBusRabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using POC.OpenTelemetry.API.Data;
using System.Reflection;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System;

namespace POC.OpenTelemetry.API.Helpers
{
    public static class StartupHelpers
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, string connectionString)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<Datacontext>(builder =>
               builder.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)));

            return services;
        }

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            return services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "POC.OpenTelemetry.API", Version = "v1" });
            });
        }
       
        public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services)
        {
           return services.AddOpenTelemetryTracing((builder) => {
                builder.AddAspNetCoreInstrumentation(opt =>
                {
                    opt.RecordException = true;
                })
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("POC.OpenTelemetry.API"))
                    .AddSource(nameof(MessageSender))
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options => options.SetDbStatementForText = true)
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