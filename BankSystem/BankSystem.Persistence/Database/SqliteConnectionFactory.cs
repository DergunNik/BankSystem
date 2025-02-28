using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Persistence.Database
{
    public class SqliteConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        private string _connectionString = connectionString;
        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
            return connection;
        }
    }
}
