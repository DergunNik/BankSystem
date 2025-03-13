using BankSystem.Application.Services;
using BankSystem.Application.Settings;
using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankSystem.Application
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
