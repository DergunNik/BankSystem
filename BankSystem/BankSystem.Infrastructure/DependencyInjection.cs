using BankSystem.Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BankSystem.Infrastructure.Persistence.Data;
using BankSystem.Infrastructure.Persistence.UnitOfWork;
using BankSystem.Infrastructure.Persistence.Settings;
using BankSystem.Infrastructure.Daemons;

namespace BankSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var rawConnectionString = configuration.GetSection("DbConnectionSettings")["SqliteConnection"];
            var resolvedConnectionString = string.Format(rawConnectionString, AppContext.BaseDirectory);
            var cleanedPath = resolvedConnectionString.Replace("Data Source = ", "").Trim();
            var targetDirectory = Path.GetDirectoryName(cleanedPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            services.Configure<DbConnectionSettings>(configuration.GetSection("DbConnectionSettings"))
                    .AddScoped<IUnitOfWork, EfUnitOfWork>()
                    .AddScoped<AppDbContext>()
                    .AddHostedService<SalaryDaemon>()
                    .AddHostedService<CreditDaemon>();
            return services;
        }
    }
}
