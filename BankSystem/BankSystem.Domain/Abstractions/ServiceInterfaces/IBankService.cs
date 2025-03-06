using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Abstractions.ServiceInterfaces
{
    public interface IBankService
    {
        Task<bool> DoesBankWithIdExistAsync(int id);
        Task AddBankAsync(Bank bank);
    }
}
