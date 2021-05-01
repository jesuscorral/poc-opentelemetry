using EventBusRabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POC.OpenTelemetry.API.Helpers;

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
            string connectionString = Configuration.GetConnectionString("poc-opentelemetry");
            var rabbitMqConfiguration = Configuration.GetSection(nameof(RabbitMqConfiguration)).Get<RabbitMqConfiguration>();

            services
                .AddControllers()
                .Services
                .AddDatabaseContext(connectionString)
                .AddCustomRabbitMQ(rabbitMqConfiguration)
                .AddCustomSwagger()
                .AddCustomOpenTelemetry();
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
