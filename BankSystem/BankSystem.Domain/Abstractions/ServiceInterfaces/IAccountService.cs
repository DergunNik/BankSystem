using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Abstractions.ServiceInterfaces
{
    public interface IAccountService
    {
        Task CreateAccountAsync(Account account);
        Task<Account?> GetAccountAsync(int accountId);
        Task ApplyMonthlyInterestAsync(int accountId);
        Task DeleteAccountAsync(int accountId);
        Task BlockAccountAsync(int accountId);
        Task UnblockAccountAsync(int accountId);
        Task FreezeAccountAsync(int accountId);
        Task UnfreezeAccountAsync(int accountId);
        Task<(IReadOnlyCollection<Transfer>, IReadOnlyCollection<BankTransfer>)>
            GetAccountTransfersAsync(int accountId);
        Task<IReadOnlyCollection<Account>>
            GetAccountFromBankAsync(int bankId);
        Task<IReadOnlyCollection<Account>> GetUserAccountsAsync(int userId);
        bool CanWithdrawFrom(Account account);
        bool CanDepositTo(Account account);
    }
}
