using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class EnterpriseService : IEnterpriseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EnterpriseService> _logger;

        public EnterpriseService(IUnitOfWork unitOfWork, ILogger<EnterpriseService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task AddExternalSpecialistAsync(int userId, int enterpriseId)
        {
            _logger.LogInformation($"AddExternalSpecialistAsync {userId} {enterpriseId}");
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(userId)
                ?? throw new Exception($"Invalid user id {userId}");
            var enterprise = await _unitOfWork.GetRepository<User>().GetByIdAsync(userId)
               ?? throw new Exception($"Invalid enterprise id {enterpriseId}");

            if (!user.IsApproved || !user.IsActive)
            {
                throw new Exception($"User {userId} is not approved or is not active");
            }
            if (user.UserRole != UserRole.ExternalSpecialist)
            {
                throw new Exception($"User {userId} is not external specialist");
            }

            var enterprises = await _unitOfWork.GetRepository<ExternalSpecialist>()
                .ListAsync(e => e.SpecielistId == userId);
            if (enterprises.Count > 0)
            {
                throw new Exception($"User {userId} is alredy some enterprise specialist");
            }

            if (user.BankId != enterprise.BankId)
            {
                throw new Exception($"User {userId} and enterprise {enterpriseId} are from different banks");
            }

            var spec = new ExternalSpecialist
            {
                SpecielistId = userId,
                EnterpriseId = enterpriseId,
                BankId = user.BankId
            };
            _unitOfWork.BeginTransaction();
            await _unitOfWork.GetRepository<ExternalSpecialist>().AddAsync(spec);
            await _unitOfWork.CommitTransactionAsync();
        }

        public async Task RemoveExternalSpecialistAsync(int userId)
        {
            _logger.LogInformation($"RemoveExternalSpecialistAsync {userId}");
            var specs = await _unitOfWork.GetRepository<ExternalSpecialist>()
                .ListAsync(s => s.SpecielistId == userId);
            foreach (var spec in specs)
            {
                try 
                {
                    _unitOfWork.BeginTransaction();
                    await _unitOfWork.GetRepository<ExternalSpecialist>().DeleteAsync(spec);
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"RemoveExternalSpecialistAsync {userId}");
                }
            }
        }

        public async Task<Enterprise?> GetExternalSpecialistEnterpriseAsync(int userId)
        {
            _logger.LogInformation($"GetExternalSpecialistEnterpriseAsync {userId}");
            var spec = await _unitOfWork.GetRepository<ExternalSpecialist>()
                .FirstOrDefaultAsync(s => s.SpecielistId == userId);
            if (spec is null) return null;
            return await _unitOfWork.GetRepository<Enterprise>().GetByIdAsync(spec.EnterpriseId);
        }

        public async Task<Enterprise?> GetEnterpriseAsync(int enterpriseId)
        {
            return await _unitOfWork.GetRepository<Enterprise>().GetByIdAsync(enterpriseId);
        }
    }
}
