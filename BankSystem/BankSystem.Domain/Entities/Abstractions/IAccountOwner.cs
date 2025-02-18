using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities.Abstractions
{
    public interface IAccountOwner
    {
        public ICollection<Account> Accounts { get; set; }
        public Enums.AccountOwnerType type { get; set; }
    }
}
