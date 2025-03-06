using BankSystem.Domain.Abstractions.ServiceInterfaces;
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
        private readonly ICreditService _creditService;
        private readonly ILogger<CreditDaemon> _logger;

        public CreditDaemon(ICreditService creditService, ILogger<CreditDaemon> logger)
        {
            _creditService = creditService;
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
                    _logger.LogInformation($"CreditDaemon: {DateTime.UtcNow}");
                    // TODO Logic;
                }
            }
        }
    }
}
