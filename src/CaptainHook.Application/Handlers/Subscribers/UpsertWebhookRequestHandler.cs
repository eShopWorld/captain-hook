using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Gateways;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using MediatR;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class UpsertWebhookRequestHandler : IRequestHandler<UpsertWebhookRequest, OperationResult<Guid>>
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IDirectorServiceGateway _directorService;

        public UpsertWebhookRequestHandler(ISubscriberRepository subscriberRepository, IDirectorServiceGateway directorService)
        {
            _subscriberRepository = subscriberRepository;
            _directorService = directorService;
        }

        public async Task<OperationResult<Guid>> Handle(UpsertWebhookRequest request, CancellationToken cancellationToken)
        {
            var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
            var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

            if (existingItem.IsError)
            {
                return new OperationResult<Guid>(existingItem.Error);
            }

            var subscriber = existingItem.Data ?? new SubscriberEntity(request.SubscriberName, null, new EventEntity(request.EventName));
            var authDto = request.Endpoint.Authentication;
            var secretStoreEntity = new SecretStoreEntity(authDto.ClientSecret.Vault, authDto.ClientSecret.Name);
            var authenticationEntity = new AuthenticationEntity(authDto.ClientId, secretStoreEntity, authDto.Uri, authDto.Type, authDto.Scopes.ToArray());
            var endpoint = new EndpointEntity(request.Endpoint.Uri, authenticationEntity, request.Endpoint.HttpVerb, null);
            subscriber.AddWebhookEndpoint(endpoint);

            var result = await _directorService.CreateReader(subscriber);
            if (result == null)
            {
                return new BusinessError("Reload in progress"); // TODO: replace with something more meaningful
            }

            return Guid.NewGuid();
        }
    }
}