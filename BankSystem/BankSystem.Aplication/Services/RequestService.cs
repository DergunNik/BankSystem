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

        public void ApproveRequest(IRequestable requestTarget)
        {
            throw new NotImplementedException();
        }

        public void CreateRequest(IRequestable requestTarget)
        {
            _unitOfWork.GetRepository<Type(requestTarget.GetType().Name + "Request")>();

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
