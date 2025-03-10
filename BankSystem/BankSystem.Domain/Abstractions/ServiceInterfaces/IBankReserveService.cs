using BankSystem.BankClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface IBankReserveService
    {
        Task TransferToBankAsync(int accountId, int bankReserveId, decimal amount, Entity? entity = null);
        Task TransferFromBankAsync(int accountId, int bankReserveId, decimal amount, Entity? entity = null);
        Task TransferToAccountBankAsync(int accountId, decimal amount, Entity? entity = null);
        Task TransferFromAccountBankAsync(int accountId, decimal amount, Entity? entity = null);
        Task GetMoneyFromStateBankAsync(int bankReserveId, decimal amount);
        Task<BankTransfer?> GetBankTransferAsync(int id);
        Task<IReadOnlyList<BankTransfer>> GetUserBankTransfersAsync(int id);
    }
}
