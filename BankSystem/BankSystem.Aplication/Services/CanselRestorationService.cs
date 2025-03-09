using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class CanselRestorationService : ICanselRestorationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransferService _transferService;
        private readonly ICreditService _creditService;
        private readonly ILogger<CanselRestorationService> _logger;

        public CanselRestorationService(
            IUnitOfWork unitOfWork, 
            ITransferService transferService,
            ICreditService creditService,
            ILogger<CanselRestorationService> logger)
        {
            _unitOfWork = unitOfWork;
            _transferService = transferService;
            _creditService = creditService;
            _logger = logger;
        }

        public async Task RestoreCansellationAsync(int canselId)
        {
            _logger.LogInformation($"RestoreCansellationAsync {canselId}");
            var cansel = await _unitOfWork.GetRepository<Cansel>().GetByIdAsync(canselId)
                ?? throw new Exception($"Invalid cansel id {canselId}");

            if (cansel.IsCanselled) throw new Exception("Already canselled");

            ICansellable canselledEntity;
            switch (cansel.CansellationType)
            {
                case CansellationType.Transfer:

                    var transfer = await _unitOfWork.GetRepository<Transfer>().GetByIdAsync(cansel.CanselledEntityId)
                        ?? throw new Exception($"Invalid transfer id {cansel.CanselledEntityId}");
                    if (!transfer.IsCanselled) throw new Exception("Alredy restored cansellation");
                    await _transferService.TransferAsync(
                        transfer.SourceAccountId, 
                        transfer.DestinationAccountId, 
                        transfer.Amount);
                    canselledEntity = transfer;
                    break;
                case CansellationType.Credit:
                    var credit = await _unitOfWork.GetRepository<Credit>().GetByIdAsync(cansel.CanselledEntityId)
                        ?? throw new Exception($"Invalid credit id {cansel.CanselledEntityId}");
                    if (!credit.IsCanselled) throw new Exception("Alredy restored cansellation");
                    await _creditService.InitCreditAsync(credit.Id);
                    canselledEntity = credit;
                    break;
                default:
                    throw new Exception("Undefined CansellationType");
            }

            try
            {
                _unitOfWork.BeginTransaction();
                cansel.IsCanselled = true;
                canselledEntity.IsCanselled = false;
                await _unitOfWork.GetRepository<Cansel>().UpdateAsync(cansel);
                switch (cansel.CansellationType)
                {
                    case CansellationType.Transfer:
                        await _unitOfWork.GetRepository<Transfer>().UpdateAsync(canselledEntity as Transfer);
                        break;
                    case CansellationType.Credit:
                        await _unitOfWork.GetRepository<Credit>().UpdateAsync(canselledEntity as Credit);
                        break;
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
            }
        }

        public async Task<Cansel?> GetCanselAsync(int id)
        {
            return await _unitOfWork.GetRepository<Cansel>().GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<Cansel>> GetCanselsFromBankAsync(int bankId)
        {
            return (await _unitOfWork.GetRepository<Cansel>().ListAsync(c => c.BankId == bankId))
                .ToList().AsReadOnly();
        }
    }
}
