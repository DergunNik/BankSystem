using BankSystem.Domain.Entities;
using BankSystem.Persistence.Database;
using BrigadeManager.Domain.Abstractions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace BankSystem.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        private IDbConnection _connection;
        private readonly IDbTransaction? _transaction;

        public Repository(IDbConnection connection, IDbTransaction? transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken _ = default)
        {
            var query = $"SELECT * FROM {typeof(T).Name} WHERE Id = @Id";
            var result = await _connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id }, _transaction);
            return result;  
        }

        public async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken _ = default)
        {
            var query = $"SELECT * FROM {typeof(T).Name}";
            var result = await _connection.QueryAsync<T>(query, transaction: _transaction);
            return result.ToList().AsReadOnly();
        }

        public async Task AddAsync(T entity, CancellationToken cance_llationToken = default)
        {
            var tableName = typeof(T).Name;
            var properties = typeof(T).GetProperties()
                                      .Where(p => p.Name != "Id") 
                                      .ToArray();

            var columnNames = properties.Select(p => p.Name).ToArray();
            var paramNames = properties.Select(p => "@" + p.Name).ToArray();

            var query = new StringBuilder($"INSERT INTO {tableName} (");
            query.Append(string.Join(", ", columnNames));
            query.Append(") VALUES (");
            query.Append(string.Join(", ", paramNames));
            query.Append(");");

            var parameters = new DynamicParameters();
            foreach (var property in properties)
            {
                parameters.Add("@" + property.Name, property.GetValue(entity));
            }

            await _connection.ExecuteAsync(query.ToString(), parameters, _transaction);
        }

        public async Task UpdateAsync(T entity, CancellationToken _ = default)
        {
            var tableName = typeof(T).Name;
            var properties = typeof(T).GetProperties()
                                      .Where(p => p.Name != "Id")
                                      .ToArray();

            var columnNames = properties.Select(p => p.Name).ToArray();
            var paramNames = properties.Select(p => "@" + p.Name).ToArray();

            var query = new StringBuilder($"UPDATE {tableName} \n SET ");
            foreach (var property in properties)
            {
                query.Append($"{property.Name} = @{property.Name}, ");
            }
            if (query.Length > 0)
            {
                query.Remove(query.Length - 2, 2);
            }
            query.Append(" WHERE Id = @Id");

            var parameters = new DynamicParameters(); 
            foreach (var property in properties)
            {
                parameters.Add("@" + property.Name, property.GetValue(entity));
            }

            await _connection.ExecuteAsync(query.ToString(), parameters, _transaction);
        }

        public async Task<IReadOnlyList<T>> ListWhere(string column, object value, CancellationToken _ = default)
        {
            var query = $"SELECT * FROM {typeof(T).Name} WHERE {column} = @{column}";
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add(column, value);
                var res = await _connection.QueryAsync<T>(query, parameters, _transaction);
                return res.ToList().AsReadOnly();
            }
            catch (Exception ex)
            {
                return Array.Empty<T>().ToList().AsReadOnly();
            }
        }

        public async Task DeleteAsync(T entity, CancellationToken _ = default)
        {
            var query = $"DELETE FROM {typeof(T).Name} WHERE Id = @Id";
            await _connection.ExecuteAsync(query, new { Id = entity.Id }, _transaction);
        }
    }
}
