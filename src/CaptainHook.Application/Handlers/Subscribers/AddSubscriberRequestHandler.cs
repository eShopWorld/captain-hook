using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class AddSubscriberRequestHandler : IRequestHandler<AddSubscriberRequest, OperationResult<Guid>>
    {
        public async Task<OperationResult<Guid>> Handle(AddSubscriberRequest request, CancellationToken cancellationToken)
        {
            if (request.Name == "error")
            {
                return new BusinessError("Error is not a valid name!");
            }

            return Guid.NewGuid();
        }
    }
}
