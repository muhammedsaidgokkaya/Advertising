using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Core.Domain.User;
using Core.Domain.Meta;

namespace Core.Data
{
    public class Context : DbContext
    {
        public Context()
        {
        }

        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Default Connection bulunmamaktadır.");
                }

                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        public DbSet<User> User { get; set; }
        public DbSet<MetaAccess> MetaAccess { get; set; }
        public DbSet<MetaLongAccess> MetaLongAccess { get; set; }
    }
}
