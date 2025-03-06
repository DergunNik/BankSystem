using BankSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Transfer : BankRelatedEntity, ICansellable
    {
        public int SourceAccountId { get; set; }
        public int DestinationAccountId { get; set; }
        public bool IsCanselled { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransferDate { get; set; }
    }
}
