using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Bank : Abstactions.Entity
    {
        public string LegalName { get; set; }
        public string BIC { get; set; }
        public string Address { get; set; }
        public ICollection<IndividualUser>? Users { get; set; }
        public ICollection<Account>? Accounts { get; set; }
        public ICollection<Loan>? Loans { get; set; }
        public ICollection<Installment>? Installments { get; set; }
        public ICollection<Transfer>? Transfers { get; set; }
    }

}
