using BankSystem.Aplication.ServiceInterfaces;
using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class RequestService : IRequestService
    {
        public RequestService(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }

        readonly IUnitOfWork _unitOfWork;

        public async Task ApproveRequestAsync(IRequestable requestTarget)
        {
            await HandleRequestAsync(true, requestTarget);
        }

        public async Task RejectRequestAsync(IRequestable requestTarget)
        {
            await HandleRequestAsync(false, requestTarget);
        }

        public async Task CreateRequestAsync(IRequestable requestTarget)
        {
            _unitOfWork.BeginTransaction();

            var (repositoryType, typedRepository) = GetRepository(requestTarget);

            var addMethod = repositoryType.GetMethod("AddAsync");
            if (addMethod != null)
            {
                requestTarget.RequestDate = DateTime.UtcNow;
                await (Task)addMethod.Invoke(typedRepository, new object[] { requestTarget });
            }

            await _unitOfWork.CommitTransactionAsync();
        }

        public IReadOnlyList<IRequestable> GetAllRequests()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IRequestable> GetRelevantRequests(UserRole recepient)
        {
            throw new NotImplementedException();
        }

        private async Task HandleRequestAsync(bool isApproved, IRequestable requestTarget)
        {
            _unitOfWork.BeginTransaction();

            dynamic? entityRepository = null;
            var requestRepository = _unitOfWork.GetRepository<Request>();
            RequestType? requestType = null;

            if (requestTarget is User)
            {
                entityRepository = _unitOfWork.GetRepository<User>();
                requestType = RequestType.User;
            }
            else if (requestTarget is Credit)
            {
                entityRepository = _unitOfWork.GetRepository<Credit>();
                requestType = RequestType.Credit;
            }
            else
            {
                throw new Exception("Not supported type is requestable");
            }

            if (isApproved)
            {
                requestTarget.IsApproved = true;
                requestTarget.AnswerDate = DateTime.UtcNow;
                await entityRepository?.UpdateAsync(requestTarget);
            }
            else
            {
                await entityRepository?.DeleteAsync(requestTarget);
            }
            var request = await requestRepository.FirstOrDefaultAsync(
                    r => r.RequestType == requestType && r.RequestEntityId == requestTarget.Id);
            if (request is not null)
            {
                request.IsChecked = true;
                await requestRepository.UpdateAsync(request);
            }

            await _unitOfWork.CommitTransactionAsync();
        }

        private (Type, dynamic?) GetRepository(IRequestable requestTarget)
        {
            Type targetType = requestTarget.GetType();
            var repositoryType = typeof(IRepository<>).MakeGenericType(targetType);
            var repository = _unitOfWork.GetType()
                .GetMethod("GetRepository")
                .MakeGenericMethod(targetType)
                .Invoke(_unitOfWork, null);
            dynamic typedRepository = Convert.ChangeType(repository, repositoryType);
            return (repositoryType, typedRepository);
        }
    }
}
