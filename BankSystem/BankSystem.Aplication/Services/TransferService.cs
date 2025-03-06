using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class TransferService : ITransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountService _accountService;
        private readonly ILogger<TransferService> _logger;

        public TransferService(IAccountService accountService, ILogger<TransferService> logger, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _accountService = accountService;
            _logger = logger;
        }

        public async Task TransferAsync(int sourceAccountId, int destinationAccountId, decimal amount)
        {
            _logger.LogInformation($"TransferAsync {sourceAccountId} -> {destinationAccountId} {amount}");
            var sourceAccount = await _accountService.GetAccountAsync(sourceAccountId);
            var destinationAccount = await _accountService.GetAccountAsync(destinationAccountId);
            
            if (sourceAccount is null || destinationAccount is null)
            {
                throw new Exception("Invalis account id");
            }
            if (!_accountService.CanWithdrawFrom(sourceAccount))
            {
                throw new Exception($"Can't withdraw from {sourceAccountId}");
            }
            if (!_accountService.CanDepositTo(destinationAccount))
            {
                throw new Exception($"Can't deposit to {destinationAccountId}");
            }
            if (sourceAccount.Balance < amount)
            {
                throw new Exception($"Source account does not have enough money");
            }

            try
            {
                _unitOfWork.BeginTransaction();
                var accountRepository = _unitOfWork.GetRepository<Account>();
                sourceAccount.Balance -= amount;
                destinationAccount.Balance += amount;
                await accountRepository.UpdateAsync(sourceAccount);
                await accountRepository.UpdateAsync(destinationAccount);
                var transfer = new Transfer()
                {
                    Amount = amount,
                    SourceAccountId = sourceAccountId,
                    DestinationAccountId = destinationAccountId,
                    TransferDate = DateTime.UtcNow
                };

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.CommitTransactionAsync();
                throw;
            }
        }
    }
}
