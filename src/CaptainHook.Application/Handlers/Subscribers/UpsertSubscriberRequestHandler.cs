using System;
using System.Linq;
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
    public class UpsertSubscriberRequestHandler: IRequestHandler<UpsertSubscriberRequest, OperationResult<SubscriberDto>>
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IDirectorServiceProxy _directorService;

        public UpsertSubscriberRequestHandler(ISubscriberRepository subscriberRepository, IDirectorServiceProxy directorService)
        {
            _subscriberRepository = subscriberRepository;
            _directorService = directorService;
        }

        public async Task<OperationResult<SubscriberDto>> Handle(UpsertSubscriberRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
                var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

                if (!(existingItem.Error is EntityNotFoundError))
                {
                    return new BusinessError("Updating subscribers not supported!");
                }

                var subscriber = MapRequestToEntity(request);

                var directorResult = await _directorService.RefreshReaderAsync(subscriber);
                if (directorResult.IsError)
                {
                    return directorResult.Error;
                }

                var saveResult = await _subscriberRepository.AddSubscriberAsync(subscriber);
                if (saveResult.IsError)
                {
                    return saveResult.Error;
                }

                return request.Subscriber;
            }
            catch (Exception ex)
            {
                return new UnhandledExceptionError($"Error processing {nameof(UpsertSubscriberRequest)}", ex);
            }
        }

        private static SubscriberEntity MapRequestToEntity(UpsertSubscriberRequest request)
        {
            var webhooks = new WebhooksEntity(
                request.Subscriber.Webhooks.SelectionRule,
                request.Subscriber.Webhooks.Endpoints?.Select(MapEndpointEntity) ?? Enumerable.Empty<EndpointEntity>());
            var subscriber = new SubscriberEntity(
                    request.SubscriberName,
                    new EventEntity(request.EventName))
                .AddWebhooks(webhooks);

            return subscriber;
        }

        private static EndpointEntity MapEndpointEntity(EndpointDto endpointDto)
        {
            var authDto = endpointDto.Authentication;
            var secretStoreEntity = new SecretStoreEntity(authDto.ClientSecret.Vault, authDto.ClientSecret.Name);
            var authenticationEntity = new AuthenticationEntity(authDto.ClientId, secretStoreEntity, authDto.Uri, authDto.Type, authDto.Scopes.ToArray());
            var uriTransformEntity = endpointDto.UriTransform != null ? new UriTransformEntity(endpointDto.UriTransform.Replace) : null;
            var endpoint = new EndpointEntity(endpointDto.Uri, authenticationEntity, endpointDto.HttpVerb, endpointDto.Selector, uriTransform: uriTransformEntity);

            return endpoint;
        }
    }
}