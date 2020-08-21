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
using Eshopworld.Core;
using Kusto.Cloud.Platform.Utils;
using MediatR;
using Polly;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class UpsertWebhookRequestHandler : IRequestHandler<UpsertWebhookRequest, OperationResult<EndpointDto>>
    {
        private static readonly TimeSpan[] DefaultRetrySleepDurations = {
            TimeSpan.FromSeconds(1.0),
            TimeSpan.FromSeconds(2.0),
        };

        private readonly ISubscriberRepository _subscriberRepository;

        private readonly IDirectorServiceProxy _directorService;

        private readonly IBigBrother _bigBrother;

        private readonly TimeSpan[] _retrySleepDurations;

        public UpsertWebhookRequestHandler(
            ISubscriberRepository subscriberRepository,
            IDirectorServiceProxy directorService,
            IBigBrother bigBrother,
            TimeSpan[] sleepDurations = null)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _directorService = directorService ?? throw new ArgumentNullException(nameof(directorService));
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
            _retrySleepDurations = sleepDurations?.SafeFastNullIfEmpty() ?? DefaultRetrySleepDurations;
        }

        public async Task<OperationResult<EndpointDto>> Handle(
            UpsertWebhookRequest request,
            CancellationToken cancellationToken)
        {
            async Task<OperationResult<EndpointDto>> AddOrUpdateEndpointAsync()
            {
                var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
                var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

                if (existingItem.Error is EntityNotFoundError)
                {
                    return await AddAsync(request, cancellationToken);
                }

                if (!existingItem.IsError)
                {
                    return await UpdateAsync(request, existingItem.Data);
                }

                return existingItem.Error;
            }

            var executionResult = await Policy
                .HandleResult<OperationResult<EndpointDto>>(result => result.Error is CannotUpdateEntityError)
                .WaitAndRetryAsync(_retrySleepDurations)
                .ExecuteAsync(AddOrUpdateEndpointAsync);

            return executionResult;
        }

        private async Task<OperationResult<EndpointDto>> AddAsync(
            UpsertWebhookRequest request,
            CancellationToken cancellationToken)
        {
            var subscriberResult = MapRequestToSubscriber(request);
            if (subscriberResult.IsError)
            {
                return subscriberResult.Error;
            }

            var subscriber = subscriberResult.Data;
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
            SubscriberEntity existingItem)
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
        }

        private static EndpointEntity MapRequestToEndpoint(UpsertWebhookRequest request, SubscriberEntity parent)
        {
            var authDto = request.Endpoint.Authentication;
            var authenticationEntity = new AuthenticationEntity(authDto.ClientId, authDto.ClientSecretKeyName, authDto.Uri, authDto.Type, authDto.Scopes.ToArray());
            var uriTransformEntity = request.Endpoint?.UriTransform != null ? new UriTransformEntity(request.Endpoint.UriTransform.Replace) : null;
            var endpoint = new EndpointEntity(request.Endpoint.Uri, authenticationEntity, request.Endpoint.HttpVerb, request.Endpoint.Selector, parent, uriTransformEntity);

            return endpoint;
        }

        private static OperationResult<SubscriberEntity> MapRequestToSubscriber(UpsertWebhookRequest request)
        {
            var subscriber = new SubscriberEntity(request.SubscriberName, new EventEntity(request.EventName));
            var endpoint = MapRequestToEndpoint(request, subscriber);
            var addWebhookResult = subscriber.AddWebhookEndpoint(endpoint);

            return addWebhookResult?.Error ?? addWebhookResult;
        }
    }
}