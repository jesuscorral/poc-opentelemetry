using EventBusRabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace POC.OpenTelemetry.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "POC.OpenTelemetry.API", Version = "v1" });
            });

            AddRabbitMQ(services);

        }

        private void AddRabbitMQ(IServiceCollection services)
        {
            var rabbitMqConfiguration = Configuration.GetSection(nameof(RabbitMqConfiguration)).Get<RabbitMqConfiguration>();

            services.AddSingleton<IEventBusRabbitMQService, EventBusRabbitMQService>(sp =>
            {
                return new EventBusRabbitMQService(rabbitMqConfiguration);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "POC.OpenTelemetry.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                var rabbitMqClient = app.ApplicationServices.GetRequiredService<IEventBusRabbitMQService>();
                rabbitMqClient.CloseConnection();
            });
        }
    }
}
