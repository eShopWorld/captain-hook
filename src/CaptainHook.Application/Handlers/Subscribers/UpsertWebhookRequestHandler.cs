using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Common.Remoting;
using CaptainHook.Common.Remoting.Types;
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
        private readonly IDirectorServiceRemoting _directorService;
        private readonly ISecretProvider _secretProvider;

        public UpsertWebhookRequestHandler(ISubscriberRepository subscriberRepository, IDirectorServiceRemoting directorService, ISecretProvider secretProvider)
        {
            _subscriberRepository = subscriberRepository;
            _directorService = directorService;
            _secretProvider = secretProvider;
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

            var mapper = new SubscriberEntityToConfigurationMapper(_secretProvider);
            var subscriberConfiguration = await mapper.MapSingleSubscriber(subscriber);
            var readerChangeInfo = ReaderChangeInfo.ToBeCreated(new DesiredReaderDefinition(subscriberConfiguration));

            var result = await _directorService.UpdateReader(readerChangeInfo);
            if (result == RequestReloadConfigurationResult.ReloadInProgress)
            {
                return new BusinessError("Reload in progress"); // TODO: replace with something more meaningful
            }



            return Guid.NewGuid();
        }
    }
}