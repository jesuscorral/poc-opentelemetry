using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using POC.OpenTelemetry.API.Data;
using System.Reflection;

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
    }
}
