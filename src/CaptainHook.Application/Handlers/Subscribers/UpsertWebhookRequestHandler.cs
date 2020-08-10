using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
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
        private readonly IDirectorServiceProxy _directorService;

        public UpsertWebhookRequestHandler(ISubscriberRepository subscriberRepository,
            IDirectorServiceProxy directorService)
        {
            _subscriberRepository = subscriberRepository;
            _directorService = directorService;
        }

        public async Task<OperationResult<EndpointDto>> Handle(UpsertWebhookRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
                var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

                if (existingItem.Error is EntityNotFoundError)
                {
                    return await AddAsync(request, cancellationToken);
                }

                if (!existingItem.IsError)
                {
                    return await UpdateAsync(request, existingItem.Data, cancellationToken);
                }

                return existingItem.Error;
            }
            catch (Exception ex)
            {
                return new UnhandledExceptionError($"Error processing {nameof(UpsertWebhookRequest)}", ex);
            }
        }

        private async Task<OperationResult<EndpointDto>> AddAsync(
            UpsertWebhookRequest request,
            CancellationToken cancellationToken)
        {
            var subscriber = MapRequestToSubscriber(request);

            var directorResult = await _directorService.CreateReaderAsync(subscriber);
            if (directorResult.IsError)
            {
                return directorResult.Error;
            }

            var saveResult = await _subscriberRepository.AddSubscriberAsync(subscriber);
            if (saveResult.IsError)
            {
                return saveResult.Error;
            }

            return request.Endpoint;
        }

        private async Task<OperationResult<EndpointDto>> UpdateAsync(
            UpsertWebhookRequest request,
            SubscriberEntity existingItem,
            CancellationToken cancellationToken)
        {
            var endpoint = MapRequestToEndpoint(request, existingItem);
            var addWebhookResult = existingItem.AddWebhookEndpoint(endpoint);

            if (addWebhookResult.IsError)
            {
                return addWebhookResult.Error;
            }

            var directorResult = await _directorService.UpdateReaderAsync(existingItem);
            if (directorResult.IsError)
            {
                return directorResult.Error;
            }

            var updateResult = await _subscriberRepository.UpdateSubscriberAsync(existingItem);
            if (updateResult.IsError)
            {
                return updateResult.Error;
            }

            return request.Endpoint;

            //repeat if error
        }

        private static EndpointEntity MapRequestToEndpoint(UpsertWebhookRequest request, SubscriberEntity parent)
        {
            var authDto = request.Endpoint.Authentication;
            var secretStoreEntity = new SecretStoreEntity(authDto.ClientSecret.Vault, authDto.ClientSecret.Name);
            var authenticationEntity = new AuthenticationEntity(authDto.ClientId, secretStoreEntity, authDto.Uri, authDto.Type, authDto.Scopes.ToArray());
            var uriTransformEntity = request.Endpoint?.UriTransform != null ? new UriTransformEntity(request.Endpoint.UriTransform.Replace) : null;
            var endpoint = new EndpointEntity(request.Endpoint.Uri, authenticationEntity, request.Endpoint.HttpVerb, request.Endpoint.Selector, parent, uriTransformEntity);

            return endpoint;
        }

        private static SubscriberEntity MapRequestToSubscriber(UpsertWebhookRequest request)
        {
            var subscriber = new SubscriberEntity(request.SubscriberName, new EventEntity(request.EventName));
            var endpoint = MapRequestToEndpoint(request, subscriber);
            subscriber.AddWebhookEndpoint(endpoint);

            return subscriber;
        }
    }
}