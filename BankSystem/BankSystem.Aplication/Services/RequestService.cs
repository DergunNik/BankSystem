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

        public void ApproveRequestAsync(IRequestable requestTarget)
        {
            _unitOfWork.BeginTransaction();
            var requests = _unitOfWork.GetRepository<Request>();
            Type targetType = requestTarget.GetType();
            var repositoryType = typeof(IRepository<>).MakeGenericType(targetType);
            var repository = _unitOfWork.GetType()
                                        .GetMethod("GetRepository")
                                        .MakeGenericMethod(targetType)
                                        .Invoke(_unitOfWork, null);
            _unitOfWork.CommitTransactionAsync();
        }

        public async Task CreateRequestAsync(IRequestable requestTarget)
        {
            _unitOfWork.BeginTransaction();

            Type targetType = requestTarget.GetType();
            var repositoryType = typeof(IRepository<>).MakeGenericType(targetType);
            var repository = _unitOfWork.GetType()
                .GetMethod("GetRepository")
                .MakeGenericMethod(targetType)
                .Invoke(_unitOfWork, null);

            dynamic typedRepository = Convert.ChangeType(repository, repositoryType);
            var addMethod = repositoryType.GetMethod("AddAsync");
            if (addMethod != null)
            {
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

        public void RejectRequest(IRequestable requestTarget)
        {
            throw new NotImplementedException();
        }
    }
}
