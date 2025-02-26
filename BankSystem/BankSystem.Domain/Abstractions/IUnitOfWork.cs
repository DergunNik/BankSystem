using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrigadeManager.Domain.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Account> AccountRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<Enterprise> EnterpriseRepository { get; }
        IRepository<Installment> InstallmentRepository { get; }
        IRepository<Loan> LoanRepository { get; }
        IRepository<Transfer> TransferRepository { get; }
        void Commit();
        void Rollback();
        public Task<bool> DatabaseExistsAsync();
        public Task DeleteDataBaseAsync();
        public Task CreateDataBaseAsync();
    }
}
