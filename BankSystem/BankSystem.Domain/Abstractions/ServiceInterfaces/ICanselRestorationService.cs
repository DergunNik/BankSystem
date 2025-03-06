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
    }
}
