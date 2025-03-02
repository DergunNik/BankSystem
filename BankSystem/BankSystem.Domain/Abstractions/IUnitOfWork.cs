using BankSystem.Domain.Entities;
using BankSystem.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Abstractions
{
    public interface IUnitOfWork
    {
        IRepository<T> GetRepository<T>() where T : Entity;
        void BeginTransaction();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task DeleteDataBaseAsync();
        Task CreateDataBaseAsync();
    }
}
