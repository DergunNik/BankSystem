using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Models
{
    public class BankTransfer : BankRelatedEntity
    {
        public int BankReserveId { get; set; }
        public int AccountId { get; set; }
        public bool IsIncomingToBank { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransferDate { get; set; }
    }
}
