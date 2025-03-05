using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.ServiceInterfaces
{
    public interface ISalaryService
    {
        Task AddEmployeeAsync(int userId, int salaryProjectId, decimal amount);
        Task RemoveEmployeeAsync(int userId, int salaryProjectId);
        Task<IReadOnlyCollection<Salary>> GetEnterpriseSalariesAsync(int enterpriseId);
        Task<IReadOnlyCollection<Salary>> GetUserSalariesAsync(int userId);
    }
}
