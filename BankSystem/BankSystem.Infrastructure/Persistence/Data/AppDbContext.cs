using BankSystem.Domain.Entities;
using BankSystem.Infrastructure.Persistence.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Infrastructure.Persistence.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Bank> Banks { get; set; } = null!;
        public DbSet<BankReserve> BankReserves { get; set; } = null!;
        public DbSet<BankTransfer> BankTransfers { get; set; } = null!;
        public DbSet<Cansel> Cansels { get; set; } = null!;
        public DbSet<Credit> Credits { get; set; } = null!;
        public DbSet<Enterprise> Enterprises { get; set; } = null!;
        public DbSet<Request> Requests { get; set; } = null!;
        public DbSet<Salary> Salaries { get; set; } = null!;
        public DbSet<SalaryProject> SalaryProjects { get; set; } = null!;
        public DbSet<Transfer> Transfers { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        public AppDbContext(IOptions<DbConnectionSettings> options)
                : base(new DbContextOptionsBuilder<AppDbContext>()
                       .UseSqlite(string.Format(options.Value.SqliteConnection, AppContext.BaseDirectory))
                       .Options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<T>? GetDbSet<T>() where T : Entity
        {
            return (DbSet<T>?)GetType().GetProperties()
                .FirstOrDefault(p => p.PropertyType == typeof(DbSet<T>))
                ?.GetValue(this);
        }
    }
}
