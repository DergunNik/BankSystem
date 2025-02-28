using BankSystem.Persistence.Database;
using BrigadeManager.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string dbConnectionString)
        {
            services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(dbConnectionString))
                    .AddSingleton<IDbInitializer, SqliteInitializer>()
                    .AddSingleton<IUnitOfWork, UnitOfWork.UnitOfWork>();
            return services;
        }
    }
}
