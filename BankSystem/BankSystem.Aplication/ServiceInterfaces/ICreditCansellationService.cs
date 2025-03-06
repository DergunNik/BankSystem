using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.ServiceInterfaces
{
    public interface ICreditCansellationService
    {
        Task CanselCreditAsync(int creditId);
    }
}
