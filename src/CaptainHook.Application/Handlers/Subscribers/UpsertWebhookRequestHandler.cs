using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Common.Remoting;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class UpsertWebhookRequestHandler : IRequestHandler<UpsertWebhookRequest, OperationResult<Guid>>
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IDirectorServiceRemoting _directorService;

        public UpsertWebhookRequestHandler(ISubscriberRepository subscriberRepository, IDirectorServiceRemoting directorService)
        {
            _subscriberRepository = subscriberRepository;
            _directorService = directorService;
        }

        public async Task<OperationResult<Guid>> Handle(UpsertWebhookRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}