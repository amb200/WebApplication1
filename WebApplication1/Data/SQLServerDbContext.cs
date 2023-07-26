using Microsoft.EntityFrameworkCore;
using WebApplication1.Entities;
using WebApplication1.JWTAuthentication;

namespace WebApplication1.Data
{
    public class SQLServerDbContext : DbContext
    {
        public SQLServerDbContext(DbContextOptions<SQLServerDbContext> options) : base(options)
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
                optionsBuilder.UseSqlServer("server=.;database=myDbSQLServer;trusted_connection=true;TrustServerCertificate=True;");
            }
        }
    }

}
