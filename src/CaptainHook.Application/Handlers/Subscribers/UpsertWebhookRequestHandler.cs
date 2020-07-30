using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Gateways;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using MediatR;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class UpsertWebhookRequestHandler : IRequestHandler<UpsertWebhookRequest, OperationResult<EndpointDto>>
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IDirectorServiceGateway _directorService;

        public UpsertWebhookRequestHandler(ISubscriberRepository subscriberRepository, IDirectorServiceGateway directorService)
        {
            _subscriberRepository = subscriberRepository;
            _directorService = directorService;
        }

        public async Task<OperationResult<EndpointDto>> Handle(UpsertWebhookRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
                var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

                if (existingItem.IsError)
                {
                    return existingItem.Error;
                }

                if (existingItem.Data != null)
                {
                    return new BusinessError("Updating subscribers not supported!");
                }

                var subscriber = MapRequestToEntity(request);

                var directorResult = await _directorService.CreateReader(subscriber);
                if (directorResult.IsError)
                {
                    return directorResult.Error;
                }

                var saveResult = await _subscriberRepository.SaveSubscriberAsync(subscriber);
                if (saveResult.IsError)
                {
                    return saveResult.Error;
                }

                return request.Endpoint;
            }
            catch (Exception ex)
            {
                return new UnhandledExceptionError($"Error processing {nameof(UpsertWebhookRequest)}", ex);
            }
        }

        private static SubscriberEntity MapRequestToEntity(UpsertWebhookRequest request)
        {
            var subscriber = new SubscriberEntity(request.SubscriberName, null, new EventEntity(request.EventName));
            var authDto = request.Endpoint.Authentication;
            var secretStoreEntity = new SecretStoreEntity(authDto.ClientSecret.Vault, authDto.ClientSecret.Name);
            var authenticationEntity = new AuthenticationEntity(authDto.ClientId, secretStoreEntity, authDto.Uri, authDto.Type, authDto.Scopes.ToArray());
            var endpoint = new EndpointEntity(request.Endpoint.Uri, authenticationEntity, request.Endpoint.HttpVerb, null);
            subscriber.AddWebhookEndpoint(endpoint);
            return subscriber;
        }
    }
}