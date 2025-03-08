using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Abstractions.ServiceInterfaces
{
    public interface ITransferService
    {
        Task TransferAsync(int sourceAccountId, int destinationAccountId, decimal amount);
        Task<Transfer?> GetTransferAsync(int transferId);
        Task<IReadOnlyCollection<Transfer>> GetTransferFromBank(int bankId);
        Task<IReadOnlyCollection<Transfer>> GetUserTransfersAsync(int userId);
    }
}
