using BankSystem.BankClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface ICanselRestorationService
    {
        Task RestoreCansellationAsync(int canselId);
        Task<Cansel?> GetCanselAsync(int id);
        Task<IReadOnlyList<Cansel>> GetCanselsFromBankAsync(int bankId);
    }
}
