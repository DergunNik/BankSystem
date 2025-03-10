using BankSystem.BankClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface IEnterpriseService
    {
        Task AddExternalSpecialistAsync(int userId, int enterpriseId);
        /// <summary>
        /// Requires User.Id as a parameter
        /// </summary>
        Task RemoveExternalSpecialistAsync(int userId);
        Task<Enterprise?> GetExternalSpecialistEnterpriseAsync(int userId);
        Task<Enterprise?> GetEnterpriseAsync(int enterpriseId);
        Task<IReadOnlyList<Enterprise>> GetBankEnterprisesAsync(int bankId);
    }
}
