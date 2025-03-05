using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.ServiceInterfaces
{
    public interface ICreditService
    {
        Task InitCreditAsync(int creditId);
        Task PayCreditAsync(int creditId, int accountId, decimal amount);
        Task<IReadOnlyList<Credit>> GetCreditsByUserIdAsync(int userId);
        Task<IReadOnlyList<Credit>> GetCreditsByUserIdAsync(int userId, bool isPaid);
    }
}
