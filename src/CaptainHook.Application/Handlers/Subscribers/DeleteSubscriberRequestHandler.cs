using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Common.Configuration;
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
    public class DeleteSubscriberRequestHandler : IRequestHandler<DeleteSubscriberRequest, OperationResult<SubscriberDto>>
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

        public DeleteSubscriberRequestHandler(
            ISubscriberRepository subscriberRepository,
            IDirectorServiceProxy directorService, IEntityToDtoMapper entityToDtoMapper, 
            ISubscriberEntityToConfigurationMapper entityToConfigurationMapper, 
            TimeSpan[] sleepDurations = null)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _directorService = directorService ?? throw new ArgumentNullException(nameof(directorService));
            _entityToDtoMapper = entityToDtoMapper ?? throw new ArgumentNullException(nameof(entityToDtoMapper));
            _entityToConfigurationMapper = entityToConfigurationMapper ?? throw new ArgumentNullException(nameof(entityToConfigurationMapper));
            _retrySleepDurations = sleepDurations?.SafeFastNullIfEmpty() ?? DefaultRetrySleepDurations;
        }

        public async Task<OperationResult<SubscriberDto>> Handle(DeleteSubscriberRequest request, CancellationToken cancellationToken)
        {
            var executionResult = await Policy
                .HandleResult<OperationResult<SubscriberDto>>(result => result.Error is CannotDeleteEntityError)
                .WaitAndRetryAsync(_retrySleepDurations)
                .ExecuteAsync(() => DeleteSubscriberAsync(request));

            return executionResult;
        }

        private async Task<OperationResult<SubscriberDto>> DeleteSubscriberAsync(DeleteSubscriberRequest request)
        {
            var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);

            var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);
            if (existingItem.IsError)
            {
                return existingItem.Error;
            }

            var subscriberEntity = existingItem.Data;

            return await _entityToConfigurationMapper.MapToWebhookAsync(subscriberEntity)
                .Then(webhookConfig => _directorService.CallDirectorService(new DeleteReader(webhookConfig)))
                .Then<SubscriberConfiguration, SubscriberEntity>(async _ => await DeleteDlqIfExists(subscriberEntity))
                .Then(_ => _subscriberRepository.RemoveSubscriberAsync(subscriberId))
                .Then<SubscriberId, SubscriberDto>(_ => _entityToDtoMapper.MapSubscriber(subscriberEntity));
        }

        private async Task<SubscriberEntity> DeleteDlqIfExists(SubscriberEntity subscriberEntity)
        {
            if (subscriberEntity.HasDlqHooks)
            {
                return await _entityToConfigurationMapper.MapToDlqAsync(subscriberEntity)
                    .Then(dlqConfig => _directorService.CallDirectorService(new DeleteReader(dlqConfig)))
                    .Then(_ => new OperationResult<SubscriberEntity>(subscriberEntity));
            }

            return subscriberEntity;
        }
    }
}