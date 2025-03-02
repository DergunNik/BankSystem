using BankSystem.Persistence.Database;
using BankSystem.Domain.Abstractions;
using BankSystem.Persistence.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BankSystem.Persistence.UnitOfWork;
using BankSystem.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BankSystem.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DbConnectionSettings>(configuration.GetSection("DbConnectionSettings"))
                    .AddSingleton<IUnitOfWork, EfUnitOfWork>()
                    .AddSingleton<AppDbContext>();
            return services;
        }
    }
}
