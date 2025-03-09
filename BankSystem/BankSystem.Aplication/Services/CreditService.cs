using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class CreditService : ICreditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreditService> _logger;
        private readonly IAccountService _accountService;
        private readonly IBankReserveService _bankReserveService;

        public CreditService(IUnitOfWork unitOfWork, IBankReserveService bankReserveService, 
            IAccountService accountService, ILogger<CreditService> logger)
        {
            _unitOfWork = unitOfWork;
            _bankReserveService = bankReserveService;
            _accountService = accountService;
            _logger = logger;
        }
        
        public async Task InitCreditAsync(int creditId)
        {
            _logger.LogInformation($"InitCreditAsync {creditId}");
            var credit = await _unitOfWork.GetRepository<Credit>().GetByIdAsync(creditId);
            if (credit is null) throw new Exception($"Invalid credit id {creditId}");
            await _bankReserveService.TransferFromAccountBankAsync(credit.AccountId, credit.CreditAmount);
        }

        public async Task PayCreditAsync(int creditId, int accountId, decimal amount)
        {
            _logger.LogInformation($"PayCreditAsync {creditId} {accountId} {amount}");
            var creditRepository = _unitOfWork.GetRepository<Credit>();
            var accountRepository = _unitOfWork.GetRepository<Account>();
            var credit = await creditRepository.GetByIdAsync(creditId);
            var account = await accountRepository.GetByIdAsync(accountId);
            if (credit is null || account is null)
            {
                throw new Exception("Invalid id input");
            }

            if (amount <= 0) throw new Exception("Amount should be positive");
            if (!credit.IsApproved) throw new Exception("Not approved credit");
            if (credit.IsPaid) throw new Exception("Credit has already been paid");
            if (!_accountService.CanWithdrawFrom(account)) throw new Exception($"Can't withdraw money from account {account.Id}");

            amount = amount > credit.TotalAmount - credit.PaidAmount ?
                credit.TotalAmount - credit.PaidAmount : amount;

            try
            {
                credit.PaidAmount += amount;
                if (credit.PaidAmount >= credit.TotalAmount)
                {
                    credit.IsPaid = true;
                }
                await _bankReserveService.TransferToAccountBankAsync(accountId, amount, credit);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Credit?> GetCreditAsync(int id)
        {
            return await _unitOfWork.GetRepository<Credit>().GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<Credit>> GetCreditsByUserIdAsync(int userId)
        {
            _logger.LogInformation($"GetCreditsByUserIdAsync {userId}");
            return await _unitOfWork.GetRepository<Credit>().ListAsync(c => c.UserId == userId);
        }

        public async Task<IReadOnlyList<Credit>> GetCreditsByUserIdAsync(int userId, bool isPaid)
        {
            _logger.LogInformation($"GetCreditsByUserIdAsync {userId} {isPaid}");
            return await _unitOfWork.GetRepository<Credit>().
                ListAsync(c => c.UserId == userId && c.IsPaid == isPaid);
        }

        public async Task<IReadOnlyList<Credit>> GetCreditsOfBankAsync(int bankId)
        {
            return await _unitOfWork.GetRepository<Credit>().ListAsync(c => c.BankId == bankId);
        }

        public async Task HandleTodaysCreditPaymentsAsync()
        {
            await Task.CompletedTask;
        }

        private async Task<User> GetUserAsync(int id)
        {
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(id);
            if (user is null) throw new Exception($"Invalid user id {id}");
            return user;
        }
    }
}
