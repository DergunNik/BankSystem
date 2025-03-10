using BankSystem.BankClient.Pages;
using BankSystem.BankClient.Services;
using BankSystem.BankClient.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace BankSystem.BankClient;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddSingleton<HttpClient>(sp =>
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5187")
            };
            return client;
        })
                        .AddSingleton<IAuthService, AuthService>()
                        .AddSingleton<IBankService, BankService>()
                        .AddSingleton<ITokenStorageService, TokenStorageService>()
                        .AddSingleton<IJwtService, JwtService>()
                        .AddTransient<BanksPage>()
                        .AddTransient<BanksViewModel>()
                        .AddTransient<AuthPage>()
                        .AddTransient<AuthViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
