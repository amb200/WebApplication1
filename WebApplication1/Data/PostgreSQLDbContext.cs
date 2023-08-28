using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using WebApplication1.Entities;
using WebApplication1.JWTAuthentication;

namespace WebApplication1.Data
{
    public class PostgreSQLDbContext : DbContext
    {
        [ExcludeFromCodeCoverage]
        public PostgreSQLDbContext(DbContextOptions<PostgreSQLDbContext> options) : base(options)
        {
        }
        public DbSet<Issue> Models { get; set; }
        public DbSet<LoginEvent> LoginEvents { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=myDbPostgres;User Id=postgres;Password=pass;");
            }
        }


    }
}
