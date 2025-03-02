using BankSystem.Domain.Entities;
using BankSystem.Persistence.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Persistence.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(IOptions<DbConnectionSettings> options)
                : base(new DbContextOptionsBuilder<AppDbContext>()
                       .UseSqlite(String.Format(options.Value.SqliteConnection, AppContext.BaseDirectory))
                       .Options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entityTypes = typeof(Entity).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(Entity).IsAssignableFrom(t));

            foreach (var entityType in entityTypes)
            {
                modelBuilder.Entity(entityType);
            }
        }

        public DbSet<T>? GetDbSet<T>() where T : Entity
        {
            return (DbSet<T>?)GetType().GetProperties()
                .FirstOrDefault(p => p.PropertyType == typeof(DbSet<T>))
                ?.GetValue(this);
        }
    }
}
