using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrigadeManager.Domain.Abstractions
{
    public interface IUnitOfWork
    {
        IRepository<Account> AccountRepository { get; }
        IRepository<IndividualUser> UserRepository { get; }
        IRepository<Enterprise> EnterpriseRepository { get; }
        IRepository<Installment> InstallmentRepository { get; }
        IRepository<Loan> LoanRepository { get; }
        IRepository<Transfer> TransferRepository { get; }
        public Task SaveAllAsync();
        public Task DeleteDataBaseAsync();
        public Task CreateDataBaseAsync();
    }
}
