using BankSystem.Infrastructure.Database;
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
            services.Configure<DbConnectionSettings>(configuration.GetSection("DbConnectionSettings"))
                    .AddScoped<IUnitOfWork, EfUnitOfWork>()
                    .AddScoped<AppDbContext>()
                    .AddHostedService<SalaryDaemon>();
            return services;
        }
    }
}
