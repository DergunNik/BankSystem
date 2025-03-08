using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Salary : BankRelatedEntity, IRequestable
    {
        public decimal Amount { get; set; }
        public int UserAccountId { get; set; }
        public int SalaryProjectId { get; set; }
        public bool IsApproved { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime AnswerDate { get; set; }
    }
}
