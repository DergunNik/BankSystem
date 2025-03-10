using System;
using System.Threading;
using System.Threading.Tasks;
using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BankSystem.Infrastructure.Daemons
{
    public class SalaryDaemon : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SalaryDaemon> _logger;

        public SalaryDaemon(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<SalaryDaemon> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRunTime = now.Date.AddDays(1).AddHours(2);
                var delay = nextRunTime - now;
                _logger.LogInformation($"SalaryDaemon at {nextRunTime} UTC (after {delay.TotalHours:F2} hours)");

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var salaryService = scope.ServiceProvider.GetRequiredService<ISalaryService>();

                    _logger.LogInformation($"SalaryDaemon: {DateTime.UtcNow}");
                    await salaryService.HandleTodaysSalariesAsync();
                }
            }
        }
    }
}