using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Abstractions.ServiceInterfaces
{
    public interface ICanselRestorationService
    {
        Task RestoreCansellationAsync(int canselId);
        Task<Cansel?> GetCanselAsync(int id);
        Task<IReadOnlyCollection<Cansel>> GetCanselsFromBankAsync(int bankId);
    }
}
