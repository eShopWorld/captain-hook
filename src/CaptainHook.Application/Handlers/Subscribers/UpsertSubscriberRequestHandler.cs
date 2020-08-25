using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Results;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using MediatR;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class UpsertSubscriberRequestHandler : IRequestHandler<UpsertSubscriberRequest, OperationResult<UpsertResult<SubscriberDto>>>
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IDirectorServiceProxy _directorService;

        public UpsertSubscriberRequestHandler(ISubscriberRepository subscriberRepository, IDirectorServiceProxy directorService)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _directorService = directorService ?? throw new ArgumentNullException(nameof(directorService));
        }

        public async Task<OperationResult<UpsertResult<SubscriberDto>>> Handle(UpsertSubscriberRequest request, CancellationToken cancellationToken)
        {
            var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
            var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

            var subscriber = MapRequestToEntity(request);
            OperationResult<bool> saveResult;
            UpsertType upsertType;
            if (existingItem.Error is EntityNotFoundError)
            {
                saveResult = await InsertSubscriber(subscriber);
                upsertType = UpsertType.Created;
            }
            else
            {
                saveResult = await UpdateSubscriber(subscriber);
                upsertType = UpsertType.Updated;
            }

            if (saveResult.IsError)
            {
                return saveResult.Error;
            }

            return new UpsertResult<SubscriberDto>(request.Subscriber, upsertType);
        }

        private async Task<OperationResult<bool>> InsertSubscriber(SubscriberEntity subscriber)
        {
            var directorResult = await _directorService.CreateReaderAsync(subscriber);
            if (directorResult.IsError)
            {
                return directorResult.Error;
            }

            var saveResult = await _subscriberRepository.AddSubscriberAsync(subscriber);
            return saveResult.Error;
        }

        private async Task<OperationResult<bool>> UpdateSubscriber(SubscriberEntity subscriber)
        {
            var directorResult = await _directorService.UpdateReaderAsync(subscriber);
            if (directorResult.IsError)
            {
                return directorResult.Error;
            }

            var saveResult = await _subscriberRepository.UpdateSubscriberAsync(subscriber);
            return saveResult.Error;
        }

        private static SubscriberEntity MapRequestToEntity(UpsertSubscriberRequest request)
        {
            var webhooks = new WebhooksEntity(
                request.Subscriber.Webhooks.SelectionRule,
                request.Subscriber.Webhooks.Endpoints?.Select(MapEndpointEntity) ?? Enumerable.Empty<EndpointEntity>(),
                MapUriTransformEntity(request.Subscriber.Webhooks.UriTransform));

            var subscriber = new SubscriberEntity(
                    request.SubscriberName,
                    new EventEntity(request.EventName))
                .AddWebhooks(webhooks);

            return subscriber;
        }

        private static UriTransformEntity MapUriTransformEntity(UriTransformDto uriTransformDto)
        {
            if (uriTransformDto?.Replace == null)
            {
                return null;
            }

            return new UriTransformEntity(uriTransformDto.Replace);
        }

        private static EndpointEntity MapEndpointEntity(EndpointDto endpointDto)
        {
            var authDto = endpointDto.Authentication;
            var authenticationEntity = new AuthenticationEntity(authDto.ClientId, authDto.ClientSecretKeyName, authDto.Uri, authDto.Type, authDto.Scopes.ToArray());
            var endpoint = new EndpointEntity(endpointDto.Uri, authenticationEntity, endpointDto.HttpVerb, endpointDto.Selector);

            return endpoint;
        }
    }
}