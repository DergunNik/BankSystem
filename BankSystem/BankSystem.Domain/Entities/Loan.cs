using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Loan : BankRelatedEntity
    {
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime StartDate { get; set; }
        public int DurationInMonths { get; set; }
        public decimal InterestRate { get; set; }
    }
}
