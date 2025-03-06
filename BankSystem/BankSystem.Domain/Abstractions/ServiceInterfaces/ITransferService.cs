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
    }
}
