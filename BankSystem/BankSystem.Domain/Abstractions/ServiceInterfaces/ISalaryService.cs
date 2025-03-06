using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Abstractions.ServiceInterfaces
{
    public interface ISalaryService
    {
        Task AddSalaryAsync(int UserAccountId, int salaryProjectId, decimal amount);
        Task RemoveSalaryAsync(int salaryId);
        Task<IReadOnlyCollection<Salary>> GetEnterpriseSalariesAsync(int enterpriseId);
        Task<IReadOnlyCollection<Salary>> GetUserSalariesAsync(int userId);
        Task HandleTodaysSalariesAsync();
    }
}
