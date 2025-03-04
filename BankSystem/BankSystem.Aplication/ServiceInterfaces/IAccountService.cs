using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.ServiceInterfaces
{
    interface IAccountService
    {
        Task CreateAccountAsync(Account account);
        Task<Account?> GetAccountAsync(int accountId);
        Task ApplyMonthlyInterestAsync(int accountId);
        Task BlockAccountAsync(int accountId);
        Task UnblockAccountAsync(int accountId);
        Task FreezeAccountAsync(int accountId);
        Task UnfreezeAccountAsync(int accountId);
    }
}
