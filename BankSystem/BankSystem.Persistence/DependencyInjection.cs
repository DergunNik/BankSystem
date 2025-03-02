using BankSystem.Persistence.Database;
using BankSystem.Domain.Abstractions;
using BankSystem.Persistence.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankSystem.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DbConnectionSettings>(configuration.GetSection("DbConnectionSettings"))
                    .AddScoped<IDbConnectionFactory, SqliteConnectionFactory>()
                    .AddScoped<IDbInitializer, SqliteInitializer>()
                    .AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            return services;
        }
    }
}
