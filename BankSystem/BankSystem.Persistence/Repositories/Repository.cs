using BankSystem.Domain.Entities;
using BankSystem.Persistence.Database;
using BrigadeManager.Domain.Abstractions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Persistence.Repositories
{
    public class Repository<T>(IDbConnection connection, IDbTransaction? transaction) : IRepository<T> where T : Entity
    {
        private IDbConnection _connection = connection;
        private IDbTransaction? _transaction = transaction;

        public async Task<T?> GetByIdAsync(int id, CancellationToken _)
        {
            var query = $"SELECT * FROM {typeof(T).Name} WHERE Id = @Id";
            var result = await connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id }, transaction);
            return result;  
        }

        public async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            var query = $"SELECT * FROM {typeof(T).Name}";
            var result = await connection.QueryAsync<T>(query, transaction: transaction);
            return result.ToList().AsReadOnly();
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            //var query = $"INSERT INTO {typeof(T).Name} VALUES (@Id, @Name)";
        }

        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        public async Task DeleteAsync(T entity, CancellationToken _)
        {
            var query = $"DELETE FROM {typeof(T).Name} WHERE Id = @Id";
            await connection.ExecuteAsync(query, new { Id = entity.Id }, transaction);
        }

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
    }
}
