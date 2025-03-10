using BankSystem.BankClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface ITransferService
    {
        Task TransferAsync(int sourceAccountId, int destinationAccountId, decimal amount);
        Task<Transfer?> GetTransferAsync(int transferId);
        Task<IReadOnlyList<Transfer>> GetTransferFromBank(int bankId);
        Task<IReadOnlyList<Transfer>> GetUserTransfersAsync(int userId);
    }
}
