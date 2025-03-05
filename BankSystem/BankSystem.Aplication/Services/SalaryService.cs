using BankSystem.Aplication.ServiceInterfaces;
using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SalaryService> _logger;

        public SalaryService(IUnitOfWork unitOfWork, ILogger<SalaryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task AddEmployeeAsync(int userId, int salaryProjectId, decimal amount)
        {
            _logger.LogInformation($"AddEmployeeAsync {userId} {salaryProjectId}");
            var (user, project) = await DoPreparation(userId, salaryProjectId);
            var salary = new Salary()
            {
                Amount = amount,
                UserId = userId,
                SalaryProjectId = salaryProjectId
            };

            _unitOfWork.BeginTransaction();
            await _unitOfWork.GetRepository<Salary>().AddAsync(salary);
            await _unitOfWork.CommitTransactionAsync();
        }

        public async Task RemoveEmployeeAsync(int userId, int salaryProjectId)
        {
            _logger.LogInformation($"RemoveEmployeeAsync {userId} {salaryProjectId}");
            var (user, project) = await DoPreparation(userId, salaryProjectId);
            var salary = await _unitOfWork.GetRepository<Salary>()
                .FirstOrDefaultAsync(s => s.SalaryProjectId == salaryProjectId && s.UserId == userId);
            
            if (salary is null) throw new Exception("Such salary doesn't exist");

            _unitOfWork.BeginTransaction();
            await _unitOfWork.GetRepository<Salary>().DeleteAsync(salary);
            await _unitOfWork.CommitTransactionAsync();
        }

        public async Task<IReadOnlyCollection<Salary>> GetEnterpriseSalariesAsync(int enterpriseId)
        {
            _logger.LogInformation($"GetEnterpriseSalariesAsync {enterpriseId}");
            var projects = await _unitOfWork.GetRepository<SalaryProject>().ListAsync(s => s.EnterpriseId == enterpriseId);
            List<Salary> salaries = [];
            foreach (var project in projects)
            {
                var temp = await _unitOfWork.GetRepository<Salary>().ListAsync(s => s.SalaryProjectId == project.Id);
                salaries.AddRange(temp);
            }
            return salaries;
        }

        public async Task<IReadOnlyCollection<Salary>> GetUserSalariesAsync(int userId)
        {
            _logger.LogInformation($"GetUserSalariesAsync {userId}");
            return await _unitOfWork.GetRepository<Salary>().ListAsync(s => s.UserId == userId);
        }

        private async Task<(User, SalaryProject)> DoPreparation(int userId, int salaryProjectId)
        {
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(userId);
            var project = await _unitOfWork.GetRepository<SalaryProject>().GetByIdAsync(salaryProjectId);
            if (user is null || project is null)
            {
                throw new Exception("Invalid id input");
            }
            return (user, project);
        }
    }
}
