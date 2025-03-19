using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Core.Domain.User;
using Core.Domain.Meta;
using Core.Domain.Google;
using Core.Domain.Report;
using Core.Domain.Task;
using System.Globalization;

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

        public DbSet<Organization> Organization { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<MetaApp> MetaApp { get; set; }
        public DbSet<MetaLongAccess> MetaLongAccess { get; set; }
        public DbSet<GoogleApp> GoogleApp { get; set; }
        public DbSet<GoogleAccessToken> GoogleAccessToken { get; set; }
        public DbSet<Report> Report { get; set; }
        public DbSet<Core.Domain.Task.Task> Task { get; set; }
        public DbSet<TaskUser> TaskUser { get; set; }
        public DbSet<TaskTemplate> TaskTemplate { get; set; }
        public DbSet<TaskTemplateTask> TaskTemplateTask { get; set; }
        public DbSet<TaskComment> TaskComment { get; set; }
        public DbSet<TaskLog> TaskLog { get; set; }
        public DbSet<Core.Domain.Calendar.Calendar> Calendar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRole)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRole)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}
