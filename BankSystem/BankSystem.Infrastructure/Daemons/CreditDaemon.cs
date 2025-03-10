using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Infrastructure.Daemons
{
    public class CreditDaemon : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CreditDaemon> _logger;

        public CreditDaemon(IServiceScopeFactory serviceScopeFactory, ILogger<CreditDaemon> logger)
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
                _logger.LogInformation($"CreditDaemon at {nextRunTime.ToString()} UTC (after {delay.TotalHours:F2} hours)");

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var creditService = scope.ServiceProvider.GetRequiredService<ICreditService>();
                    _logger.LogInformation($"CreditDaemon: {DateTime.UtcNow}");
                    await creditService.HandleTodaysCreditPaymentsAsync();
                }
            }
        }
    }
}
