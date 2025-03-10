using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface ICreditCansellationService
    {
        Task CanselCreditAsync(int creditId);
    }
}
