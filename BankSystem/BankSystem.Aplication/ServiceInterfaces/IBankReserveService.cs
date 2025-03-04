using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.ServiceInterfaces
{
    interface IBankReserveService
    {
        Task TransferMoneyToBankAsync(int accountId, int bankReserveId, decimal amount);
        Task TransferMoneyFromBankAsync(int accountId, int bankReserveId, decimal amount);
        Task GetMoneyFromStateBankAsync(int bankReserveId, decimal amount);
    }
}
