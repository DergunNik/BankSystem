using BankSystem.BankClient.Abstractions;
using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using BankSystem.BankClient.Enums;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransferService _transferService;
        private readonly ILogger<SalaryService> _logger;

        public SalaryService(IUnitOfWork unitOfWork, ITransferService transferService, ILogger<SalaryService> logger)
        {
            _unitOfWork = unitOfWork;
            _transferService = transferService;
            _logger = logger;
        }

        public async Task AddSalaryAsync(int userAccountId, int salaryProjectId, decimal amount)
        {
            _logger.LogInformation($"AddEmployeeAsync {userAccountId} {salaryProjectId}");
            var (account, project) = await DoPreparation(userAccountId, salaryProjectId);
            if (account.OwnerType != AccountOwnerType.IndividualUser)
            {
                throw new Exception("Can't add salary to nor individual user account");
            }   
            
            var salary = new Salary()
            {
                Amount = amount,
                UserAccountId = userAccountId,
                SalaryProjectId = salaryProjectId
            };

            _unitOfWork.BeginTransaction();
            await _unitOfWork.GetRepository<Salary>().AddAsync(salary);
            await _unitOfWork.CommitTransactionAsync();
        }

        public async Task RemoveSalaryAsync(int salaryId)
        {
            _logger.LogInformation($"RemoveEmployeeAsync {salaryId}");
            var salary = await _unitOfWork.GetRepository<Salary>().GetByIdAsync(salaryId)
                ?? throw new Exception($"Invalid salary id {salaryId}");
            
            _unitOfWork.BeginTransaction();
            await _unitOfWork.GetRepository<Salary>().DeleteAsync(salary);
            await _unitOfWork.CommitTransactionAsync();
        }

        public async Task<Salary?> GetSalaryAsync(int salaryId)
        {
            return await _unitOfWork.GetRepository<Salary>().GetByIdAsync(salaryId);
        }

        public async Task<SalaryProject?> GetSalaryProjectAsync(int projectId)
        {
            return await _unitOfWork.GetRepository<SalaryProject>().GetByIdAsync(projectId);
        }

        public async Task<IReadOnlyCollection<Salary>> GetSalariesFromBankAsync(int bankId)
        {
            return await _unitOfWork.GetRepository<Salary>().ListAsync(s => s.BankId == bankId);
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
            var accounts = _unitOfWork.GetRepository<Account>()
                .ListAsync(a => a.OwnerType == AccountOwnerType.IndividualUser && a.OwnerId == userId)
                .Result.Select(a => a.Id).ToHashSet();
            return await _unitOfWork.GetRepository<Salary>().ListAsync(s => accounts.Contains(s.Id));
        }

        public async Task HandleTodaysSalariesAsync()
        {
            _logger.LogInformation("HandleTodaysSalariesAsync");
            var projects = await _unitOfWork.GetRepository<SalaryProject>()
                .ListAsync(p => p.SalaryDate.Day == DateTime.UtcNow.Day);
            foreach (var project in projects)
            {
                int enterpriseAccountId = project.EnterpriseAccountId;
                var salaries = await _unitOfWork.GetRepository<Salary>()
                    .ListAsync(s => s.SalaryProjectId == project.Id);
                decimal salariesSum = 0m;
                foreach (var salary in salaries)
                {
                    salariesSum += salary.Amount;
                }
                var enterpriseAccount = await _unitOfWork.GetRepository<Account>().GetByIdAsync(enterpriseAccountId);
                if (salariesSum > enterpriseAccount.Balance)
                {
                    _logger.LogWarning($"Enterprice {project.EnterpriseId} doesn't have enough money for salaries");
                    continue;
                }
                foreach (var salary in salaries)
                {
                    try
                    {
                        await _transferService.TransferAsync(salary.UserAccountId, enterpriseAccount.Id, salary.Amount);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Can't pay salary {salary.Id} to {salary.UserAccountId}");
                    }
                }
            }
        }

        private async Task<(Account, SalaryProject)> DoPreparation(int userAccountId, int salaryProjectId)
        {
            var user = await _unitOfWork.GetRepository<Account>().GetByIdAsync(userAccountId)
                ?? throw new Exception($"Invalid account id {userAccountId}");
            var project = await _unitOfWork.GetRepository<SalaryProject>().GetByIdAsync(salaryProjectId)
                ?? throw new Exception($"Invalid salaru project id {salaryProjectId}");
            return (user, project);
        }
    }
}
