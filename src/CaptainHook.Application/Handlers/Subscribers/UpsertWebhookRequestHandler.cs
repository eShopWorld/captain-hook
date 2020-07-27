using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class UpsertWebhookRequestHandler : IRequestHandler<UpsertWebhookRequest, OperationResult<Guid>>
    {
        public async Task<OperationResult<Guid>> Handle(UpsertWebhookRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}