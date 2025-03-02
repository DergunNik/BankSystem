using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public interface IRequestable
    {
        bool IsApproved { get; set; }
        DateTime RequestDate { get; set; }
        DateTime AnswerDate { get; set; }
    }
}
