using BankSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Account : BankRelatedEntity
    {
        public decimal Balance { get; set; }
        public int OwnerId { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsFrozen{ get; set; }
        public bool IsSavingAccount { get; set; }
        public decimal MonthlyInterestRate { get; set; } // in percents, MonthlyInterestRate == 1.2 means 1.2%
        public DateTime CreationDate { get; set; }
        public DateTime SavingsAccountUntil { get; set; }
        public AccountOwnerType OwnerType { get; set; }
    }
}
