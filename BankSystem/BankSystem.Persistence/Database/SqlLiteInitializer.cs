using BankSystem.Domain.Entities;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Persistence.Database
{
    public class SqlLiteInitializer : IDbInitializer
    {
        private IDbConnection _connection;

        public SqlLiteInitializer(IDbConnectionFactory dbConnectionFactory)
        {
            _connection = dbConnectionFactory.CreateConnectionAsync().Result;
        }

        public async Task InitializeAsync()
        {
            _connection.Open();
            var entityTypes = GetAllEntityClasses();
            foreach(var type in entityTypes)
            {
                var query = new StringBuilder();
                query.AppendLine($"CREATE TABLE IF NOT EXISTS {type.Name}\n(");
                foreach(var field in type.GetFields())
                {
                    var datatype = field.FieldType;
                    if (datatype.IsEnum)
                    {
                        datatype = Enum.GetUnderlyingType(datatype);
                    }

                    string sqlType = datatype.Name switch
                    {
                        "Int32" => "INTEGER",
                        "String" => "TEXT",
                        "Boolean" => "INTEGER", 
                        "DateTime" => "TEXT", 
                        "Double" => "REAL",
                        "Decimal" => "REAL", 
                        "Single" => "REAL",
                        "Int64" => "INTEGER",
                        "Int16" => "INTEGER",
                        "Byte" => "INTEGER",
                        _ => "TEXT" 
                    };

                    query.AppendLine($"{field.Name} {sqlType} NOT NULL"); 
                    
                    if (field.Name == "Id")
                    {
                        query.Append(" PRIMARY KEY AUTOINCREMENT");
                    }

                    query.Append(", ");
                }

                if (query.Length > 0)
                {
                    query.Length -= 3; //to do testing
                }
                query.AppendLine(");");

                await _connection.ExecuteAsync(query.ToString());
            }

            _connection.Close();
        }

        private static List<Type> GetAllEntityClasses()
        {
            var baseType = typeof(Entity);
            var assembly = baseType.Assembly;

            var derivedTypes = assembly
                .GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    baseType.IsAssignableFrom(t))
                .ToList();

            return derivedTypes;
        }
    }
}
