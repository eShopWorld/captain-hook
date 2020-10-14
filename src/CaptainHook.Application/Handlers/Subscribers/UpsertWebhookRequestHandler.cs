using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
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
        private readonly IDtoToEntityMapper _dtoToEntityMapper;
        private readonly ISubscriberEntityToConfigurationMapper _entityToConfigurationMapper;
        private readonly TimeSpan[] _retrySleepDurations;

        public UpsertWebhookRequestHandler(
            ISubscriberRepository subscriberRepository,
            IDirectorServiceProxy directorService,
            IDtoToEntityMapper dtoToEntityMapper,
            ISubscriberEntityToConfigurationMapper entityToConfigurationMapper,
            TimeSpan[] sleepDurations = null)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _directorService = directorService ?? throw new ArgumentNullException(nameof(directorService));
            _dtoToEntityMapper = dtoToEntityMapper ?? throw new ArgumentNullException(nameof(dtoToEntityMapper));
            _entityToConfigurationMapper = entityToConfigurationMapper ?? throw new ArgumentNullException(nameof(entityToConfigurationMapper));
            _retrySleepDurations = sleepDurations?.SafeFastNullIfEmpty() ?? DefaultRetrySleepDurations;
        }

        public async Task<OperationResult<EndpointDto>> Handle(
            UpsertWebhookRequest request,
            CancellationToken cancellationToken)
        {
            var executionResult = await Policy
                .HandleResult<OperationResult<EndpointDto>>(result => result.Error is CannotUpdateEntityError)
                .WaitAndRetryAsync(_retrySleepDurations)
                .ExecuteAsync(() => AddOrUpdateEndpointAsync(request));

            return executionResult;
        }

        private async Task<OperationResult<EndpointDto>> AddOrUpdateEndpointAsync(UpsertWebhookRequest request)
        {
            var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
            var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

            if (existingItem.Error is EntityNotFoundError)
            {
                return await AddAsync(request);
            }

            if (!existingItem.IsError)
            {
                return await UpdateAsync(request, existingItem.Data);
            }

            return existingItem.Error;
        }

        private async Task<OperationResult<EndpointDto>> AddAsync(UpsertWebhookRequest request)
        {
            var subscriber = MapRequestToSubscriberEntity(request);

            if (subscriber.IsError)
            {
                return subscriber.Error;
            }

            return await _entityToConfigurationMapper.MapToWebhookAsync(subscriber)
                .Then(configuration => _directorService.CallDirectorServiceAsync(new UpdateReader(configuration)))
                .Then(_ => _subscriberRepository.AddSubscriberAsync(subscriber))
                .Then<SubscriberEntity, EndpointDto>(_ => request.Endpoint);
        }

        private async Task<OperationResult<EndpointDto>> UpdateAsync(UpsertWebhookRequest request, SubscriberEntity subscriber)
        {
            var endpoint = MapRequestToEndpointEntity(request, subscriber);

            return await subscriber.SetWebhookEndpoint(endpoint)
                .Then(_ => _entityToConfigurationMapper.MapToWebhookAsync(subscriber))
                .Then(configuration => _directorService.CallDirectorServiceAsync(new UpdateReader(configuration)))
                .Then(_ => _subscriberRepository.UpdateSubscriberAsync(subscriber))
                .Then<SubscriberEntity, EndpointDto>(_ => request.Endpoint);
        }

        private EndpointEntity MapRequestToEndpointEntity(UpsertWebhookRequest request, SubscriberEntity parent)
        {
            return _dtoToEntityMapper
                        .MapEndpoint(request.Endpoint, request.Selector)
                        .SetParentSubscriber(parent);
        }

        private OperationResult<SubscriberEntity> MapRequestToSubscriberEntity(UpsertWebhookRequest request)
        {
            var subscriber = new SubscriberEntity(request.SubscriberName, new EventEntity(request.EventName));
            var endpointEntity = MapRequestToEndpointEntity(request, subscriber);

            return subscriber.SetWebhookEndpoint(endpointEntity);
        }
    }
}