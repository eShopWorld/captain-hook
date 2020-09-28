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
    public class DeleteWebhookRequestHandler : IRequestHandler<DeleteWebhookRequest, OperationResult<SubscriberDto>>
    {
        private static readonly TimeSpan[] DefaultRetrySleepDurations = {
            TimeSpan.FromSeconds(1.0),
            TimeSpan.FromSeconds(2.0),
        };

        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IDirectorServiceProxy _directorService;
        private readonly IEntityToDtoMapper _entityToDtoMapper;
        private readonly ISubscriberEntityToConfigurationMapper _entityToConfigurationMapper;
        private readonly TimeSpan[] _retrySleepDurations;

        public DeleteWebhookRequestHandler(
            ISubscriberRepository subscriberRepository,
            IDirectorServiceProxy directorService,
            IEntityToDtoMapper entityToDtoMapper,
            ISubscriberEntityToConfigurationMapper entityToConfigurationMapper,
            TimeSpan[] sleepDurations = null)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _directorService = directorService ?? throw new ArgumentNullException(nameof(directorService));
            _entityToDtoMapper = entityToDtoMapper ?? throw new ArgumentNullException(nameof(entityToDtoMapper));
            _entityToConfigurationMapper = entityToConfigurationMapper ?? throw new ArgumentNullException(nameof(entityToConfigurationMapper));
            _retrySleepDurations = sleepDurations?.SafeFastNullIfEmpty() ?? DefaultRetrySleepDurations;
        }

        public async Task<OperationResult<SubscriberDto>> Handle(DeleteWebhookRequest request, CancellationToken cancellationToken)
        {
            var executionResult = await Policy
                .HandleResult<OperationResult<SubscriberDto>>(result => result.Error is CannotUpdateEntityError)
                .WaitAndRetryAsync(_retrySleepDurations)
                .ExecuteAsync(() => DeleteEndpointAsync(request));

            return executionResult;
        }

        private async Task<OperationResult<SubscriberDto>> DeleteEndpointAsync(DeleteWebhookRequest request)
        {
            var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
            var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

            if (existingItem.IsError)
            {
                return existingItem.Error;
            }

            var removeResult = existingItem.Data.RemoveWebhookEndpoint(EndpointEntity.FromSelector(request.Selector));
            if (removeResult.IsError)
            {
                return removeResult.Error;
            }

            var subscriberConfiguration = await _entityToConfigurationMapper.MapToWebhookAsync(existingItem);
            if (subscriberConfiguration.IsError)
            {
                return subscriberConfiguration.Error;
            }

            var directorResult = await _directorService.CallDirectorService(new UpdateReader(subscriberConfiguration));
            if (directorResult.IsError)
            {
                return directorResult.Error;
            }

            var saveResult = await _subscriberRepository.UpdateSubscriberAsync(existingItem);
            if (saveResult.IsError)
            {
                return saveResult.Error;
            }

            return _entityToDtoMapper.MapSubscriber(saveResult);
        }
    }
}