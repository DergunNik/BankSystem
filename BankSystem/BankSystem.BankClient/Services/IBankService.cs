using BankSystem.BankClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Services
{
    public interface IBankService
    {
        Task<List<Bank>> GetBanksAsync();
    }
}
