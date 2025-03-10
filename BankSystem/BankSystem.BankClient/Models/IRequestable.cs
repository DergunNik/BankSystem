using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Models
{
    public interface IRequestable
    {
        int Id { get; set; }
        bool IsApproved { get; set; }
        DateTime RequestDate { get; set; }
        DateTime AnswerDate { get; set; }
    }
}
