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

        public static IServiceCollection AddCustomRabbitMQ(this IServiceCollection services, RabbitMqConfiguration rabbitMqConfiguration)
        {

            services.AddSingleton<IEventBusRabbitMQService, EventBusRabbitMQService>(sp =>
            {
                return new EventBusRabbitMQService(rabbitMqConfiguration);
            });
            return services;

        }

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "POC.OpenTelemetry.API", Version = "v1" });
            });

            return services;
        }
        public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services)
        {
           return services.AddOpenTelemetryTracing((builder) => {
                builder.AddAspNetCoreInstrumentation(opt =>
                {
                    opt.RecordException = true;
                })
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("POC.OpenTelemetry.API"))
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options => options.SetDbStatementForText = true)
                    .AddConsoleExporter()
                    .AddZipkinExporter(options =>
                    {
                        options.Endpoint = new Uri("http://zipkin1:9411/api/v2/spans");
                    })
                    .AddJaegerExporter(jaegerOptions =>
                    {
                        jaegerOptions.AgentHost = "jaeger";
                        jaegerOptions.AgentPort = 6813;
                    });
                });
        }
    }
}
