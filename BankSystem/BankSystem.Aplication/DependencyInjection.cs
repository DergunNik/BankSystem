using BankSystem.Aplication.ServiceInterfaces;
using BankSystem.Aplication.Services;
using BankSystem.Aplication.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankSystem.Aplication
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AuthSettings>(configuration.GetSection("AuthSettings"))
                    .AddScoped<IJwtService, JwtService>()
                    .AddScoped<IAuthService, AuthService>();
            return services;
        }
    }
}
