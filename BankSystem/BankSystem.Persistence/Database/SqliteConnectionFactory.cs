using BankSystem.Persistence.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Persistence.Database
{
    public class SqliteConnectionFactory(IOptions<DbConnectionSettings> options) : IDbConnectionFactory
    {
        private string _connectionString = options.Value.SqliteConnection;
        public IDbConnection CreateConnection()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
            return connection;
        }
    }
}
