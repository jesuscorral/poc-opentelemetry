using Microsoft.EntityFrameworkCore;
using POC.OpenTelemetry.API.Domain;

namespace POC.OpenTelemetry.API.Data
{
    public class Datacontext : DbContext
    {
        public Datacontext(DbContextOptions options) : base(options)
        {
        }


        public DbSet<User> Users { get; set; }
    }
}
