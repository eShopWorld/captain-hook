using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.Mappers;
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
        private readonly IDtoToEntityMapper _dtoToEntityMapper;
        private readonly IDirectorServiceRequestsGenerator _directorServiceRequestsGenerator;
        private readonly IDirectorServiceProxy _directorServiceProxy;

        public UpsertSubscriberRequestHandler(
            ISubscriberRepository subscriberRepository,
            IDirectorServiceRequestsGenerator directorServiceRequestsGenerator,
            IDtoToEntityMapper dtoToEntityMapper,
            IDirectorServiceProxy directorServiceProxy)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _directorServiceRequestsGenerator = directorServiceRequestsGenerator ?? throw new ArgumentNullException(nameof(directorServiceRequestsGenerator));
            _dtoToEntityMapper = dtoToEntityMapper ?? throw new ArgumentNullException(nameof(dtoToEntityMapper));
            _directorServiceProxy = directorServiceProxy ?? throw new ArgumentNullException(nameof(directorServiceProxy));
        }

        public async Task<OperationResult<UpsertResult<SubscriberDto>>> Handle(UpsertSubscriberRequest request, CancellationToken cancellationToken)
        {
            var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
            var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

            if (existingItem.IsError && !(existingItem.Error is EntityNotFoundError))
            {
                return existingItem.Error;
            }

            var subscriber = MapRequestToEntity(request);

            var requests = await _directorServiceRequestsGenerator.DefineChangesAsync(subscriber, existingItem.Data);
            if (requests.IsError)
            {
                return requests.Error;
            }

            foreach (var dsRequest in requests.Data)
            {
                var directorResult = await _directorServiceProxy.CallDirectorServiceAsync(dsRequest);
                if (directorResult.IsError)
                {
                    return directorResult.Error;
                }
            }

            if (existingItem.Error is EntityNotFoundError)
            {
                return await _subscriberRepository.AddSubscriberAsync(subscriber)
                    .Then(_ => new OperationResult<UpsertResult<SubscriberDto>>(new UpsertResult<SubscriberDto>(request.Subscriber, UpsertType.Created)));
            }

            return await _subscriberRepository.UpdateSubscriberAsync(subscriber)
                .Then(_ => new OperationResult<UpsertResult<SubscriberDto>>(new UpsertResult<SubscriberDto>(request.Subscriber, UpsertType.Updated)));
        }

        private SubscriberEntity MapRequestToEntity(UpsertSubscriberRequest request)
        {
            var subscriberEntity = new SubscriberEntity(request.SubscriberName, new EventEntity(request.EventName))
            {
                MaxDeliveryCount = request.Subscriber.MaxDeliveryCount
            };

            var webhooks = _dtoToEntityMapper.MapWebooks(request.Subscriber.Webhooks, WebhooksEntityType.Webhooks);
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