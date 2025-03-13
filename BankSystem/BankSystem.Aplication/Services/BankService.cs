using BankSystem.BankClient.Abstractions;
using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Application.Services
{
    public class BankService : IBankService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BankService> _logger;
        private const decimal START_BANK_RESERVE = 10_000_000m;

        public BankService(IUnitOfWork unitOfWork, ILogger<BankService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task AddBankAsync(Bank bank)
        {
            _logger.LogInformation($"AddBankAsync {bank.ToString()}");
            if (String.IsNullOrEmpty(bank.Address) ||
                String.IsNullOrEmpty(bank.BIC) ||
                String.IsNullOrEmpty(bank.LegalName))   
            {
                throw new Exception("Can't contain empty fields");
            }

            _unitOfWork.BeginTransaction();
            await _unitOfWork.GetRepository<Bank>().AddAsync(bank);
            await _unitOfWork.CommitTransactionAsync();

            bank = await _unitOfWork.GetRepository<Bank>()
                .FirstOrDefaultAsync(b => b.Address == bank.Address && b.LegalName == bank.LegalName && b.BIC == bank.BIC)
                ?? throw new Exception("Bank creation error");

            var reserve = new BankReserve()
            {
                BankId = bank.Id,
                Balance = START_BANK_RESERVE
            };

            _unitOfWork.BeginTransaction();
            await _unitOfWork.GetRepository<BankReserve>().AddAsync(reserve);
            await _unitOfWork.CommitTransactionAsync();
        }

        public async Task<bool> DoesBankWithIdExistAsync(int id)
        {
            _logger.LogInformation($"DoesBankWithIdExistAsync {id}");
            var b = await _unitOfWork.GetRepository<Bank>().GetByIdAsync(id);
            return b is not null;
        }

        public async Task<Bank?> GetBankByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<Bank>().GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<Bank>> GetAllBanksAsync()
        {
            return await _unitOfWork.GetRepository<Bank>().ListAllAsync();
        }
    }
}
