using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Persistence.Database
{
    interface IDbConnectionFactory
    {
        public Task<IDbConnection> CreateConnectionAsync();
    }
}
