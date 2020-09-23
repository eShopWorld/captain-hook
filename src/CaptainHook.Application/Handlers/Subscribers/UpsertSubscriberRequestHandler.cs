using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Results;
using CaptainHook.Common.Configuration;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using MediatR;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class DirectorServiceChangeDecider
    {
        private readonly ISubscriberEntityToConfigurationMapper _entityToConfigurationMapper;
        private readonly IDirectorServiceProxy _directorService;

        public DirectorServiceChangeDecider(ISubscriberEntityToConfigurationMapper entityToConfigurationMapper, IDirectorServiceProxy directorService)
        {
            _entityToConfigurationMapper = entityToConfigurationMapper;
            _directorService = directorService;
        }

        public async Task<OperationResult<SubscriberConfiguration>> ApplyChangesAsync(SubscriberEntity existingEntity, SubscriberEntity requestedEntity)
        {
            var subscriberConfigsResult = await _entityToConfigurationMapper.MapSubscriberEntityAsync(requestedEntity);

            if (subscriberConfigsResult.IsError)
            {
                return subscriberConfigsResult.Error;
            }

            var keyVaultModels = subscriberConfigsResult.Data;

            if (existingEntity == null)
            {
                return await _directorService.CreateReaderAsync(keyVaultModels.Webhook)
                    .Then(async _ => await _directorService.CreateReaderAsync(keyVaultModels.Webhook));
            }

            return await _directorService.UpdateReaderAsync(keyVaultModels.Webhook)
                .Then(async _ => await UpdateDlqReader(keyVaultModels.Dlqhook, existingEntity.HasDlqHooks, requestedEntity.HasDlqHooks));
        }

        private async Task<OperationResult<SubscriberConfiguration>> UpdateDlqReader(SubscriberConfiguration dlqhook, bool alreadyExists, bool isRequired)
        {
            if (!alreadyExists && isRequired)
            {
                return await _directorService.CreateReaderAsync(dlqhook);
            }

            if (alreadyExists && !isRequired)
            {
                return await _directorService.DeleteReaderAsync(dlqhook);
            }

            return await _directorService.UpdateReaderAsync(dlqhook);
        }
    }

    public class UpsertSubscriberRequestHandler : IRequestHandler<UpsertSubscriberRequest, OperationResult<UpsertResult<SubscriberDto>>>
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IDirectorServiceProxy _directorService;
        private readonly IDtoToEntityMapper _dtoToEntityMapper;

        public UpsertSubscriberRequestHandler(
            ISubscriberRepository subscriberRepository,
            IDirectorServiceProxy directorService,
            IDtoToEntityMapper dtoToEntityMapper)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _directorService = directorService ?? throw new ArgumentNullException(nameof(directorService));
            _dtoToEntityMapper = dtoToEntityMapper ?? throw new ArgumentNullException(nameof(dtoToEntityMapper));
        }

        public async Task<OperationResult<UpsertResult<SubscriberDto>>> Handle(UpsertSubscriberRequest request, CancellationToken cancellationToken)
        {
            var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
            var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

            var subscriber = MapRequestToEntity(request);

            if (existingItem.IsError)
            {
                if (existingItem.Error is EntityNotFoundError)
                {
                    return await _directorService.CreateReaderAsync(subscriber)
                        .Then(async _ => (await _subscriberRepository.AddSubscriberAsync(subscriber)))
                        .Then(_ => new OperationResult<UpsertResult<SubscriberDto>>(new UpsertResult<SubscriberDto>(request.Subscriber, UpsertType.Created)));
                }

                return existingItem.Error;
            }

            return await _directorService.UpdateReaderAsync(subscriber)
                .Then(async _ => (await _subscriberRepository.UpdateSubscriberAsync(subscriber)))
                .Then(_ => new OperationResult<UpsertResult<SubscriberDto>>(new UpsertResult<SubscriberDto>(request.Subscriber, UpsertType.Updated)));
        }

        private SubscriberEntity MapRequestToEntity(UpsertSubscriberRequest request)
        {
            var webhooks = _dtoToEntityMapper.MapWebooks(request.Subscriber.Webhooks, WebhooksEntityType.Webhooks);

            var subscriberEntity = new SubscriberEntity(request.SubscriberName, new EventEntity(request.EventName));

            subscriberEntity.SetHooks(webhooks);

            if (request.Subscriber.Callbacks?.Endpoints?.Count > 0)
            {
                var callbacks = _dtoToEntityMapper.MapWebooks(request.Subscriber.Callbacks, WebhooksEntityType.Callbacks);
                subscriberEntity.SetHooks(callbacks);
            }

            if (request.Subscriber.DlqHooks?.Endpoints?.Count > 0)
            {
                var dlqHooks = _dtoToEntityMapper.MapWebooks(request.Subscriber.DlqHooks, WebhooksEntityType.DlqHooks);
                subscriberEntity.SetHooks(dlqHooks);
            }

            return subscriberEntity;
        }
    }
}