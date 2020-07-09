using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Domain.Common;
using CaptainHook.Domain.Requests;
using MediatR;

namespace CaptainHook.Domain.Handlers.Subscribers
{
    public class AddSubscriberRequestHandler : IRequestHandler<AddSubscriberRequest, EitherErrorOr<Guid>>
    {
        public async Task<EitherErrorOr<Guid>> Handle(AddSubscriberRequest request, CancellationToken cancellationToken)
        {
            if (request.Name == "error")
            {
                return new BusinessError("Error is not a valid name!");
            }

            return Guid.NewGuid();
        }
    }
}
