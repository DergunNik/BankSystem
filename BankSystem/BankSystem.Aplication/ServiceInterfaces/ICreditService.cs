using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.ServiceInterfaces
{
    public interface ICreditService
    {
        void AddCredit(Credit credit, User user);
        void AddCredit(Credit credit, int userId);
        void PayCredit(int creditId, int accountId, int userId);
        void PayCredit(int creditId, int accountId, User user);
        IReadOnlyList<Credit> GetCreditsByUserId(Guid userId);
    }
}
