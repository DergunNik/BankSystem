using BankSystem.BankClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface IBankService
    {
        Task<bool> DoesBankWithIdExistAsync(int id);
        Task AddBankAsync(Bank bank);
        Task<Bank?> GetBankByIdAsync(int id);
        Task<IReadOnlyList<Bank>> GetAllBanksAsync();
    }
}
