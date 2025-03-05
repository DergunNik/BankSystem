using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Salary : BankRelatedEntity
    {
        public decimal Amount { get; set; }
        public int UserId { get; set; }
        public int SalaryProjectId { get; set; }
    }
}
