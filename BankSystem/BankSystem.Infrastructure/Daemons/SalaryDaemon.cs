using BankSystem.Domain.Abstractions;
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
    public class SalaryDaemon : BackgroundService
    {
        private readonly ISalaryService _salaryService;
        private readonly ILogger<SalaryDaemon> _logger;
        
        public SalaryDaemon(ISalaryService salaryService, ILogger<SalaryDaemon> logger)
        {
            _salaryService = salaryService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRunTime = now.Date.AddDays(1).AddHours(2);
                var delay = nextRunTime - now;
                _logger.LogInformation($"SalaryDaemon at {nextRunTime.ToString()} UTC (after {delay.TotalHours:F2} hours)");

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"SalaryDaemon: {DateTime.UtcNow}");
                    await _salaryService.HandleTodaysSalariesAsync();
                }
            }
        }
    }
}
