using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Models
{
    public interface ICansellable 
    {
        bool IsCanselled { get; set; }
    }
}
