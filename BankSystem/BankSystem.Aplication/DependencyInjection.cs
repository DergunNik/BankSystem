using BankSystem.Aplication.Services;
using BankSystem.Aplication.Settings;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankSystem.Aplication
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AuthSettings>(configuration.GetSection("AuthSettings"))
                    .AddScoped<IAccountService, AccountService>()
                    .AddScoped<IAuthService, AuthService>()
                    .AddScoped<IBankReserveService, BankReserveService>()
                    .AddScoped<IBankService, BankService>()
                    .AddScoped<ICanselRestorationService, CanselRestorationService>()
                    .AddScoped<ICreditCansellationService, CreditCansellationService>()
                    .AddScoped<ICreditService, CreditService>()
                    .AddScoped<IEnterpriseService, EnterpriseService>()
                    .AddScoped<IJwtService, JwtService>()
                    .AddScoped<IRequestService, RequestService>()
                    .AddScoped<ISalaryService, SalaryService>()
                    .AddScoped<ITransferCansellationService, TransferCansellationService>()
                    .AddScoped<ITransferService, TransferService>()
                    .AddScoped<IUserService, UserService>();
            return services;
        }
    }
}
