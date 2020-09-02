using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
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
    public class DeleteSubscriberRequestHandler : IRequestHandler<DeleteSubscriberRequest, OperationResult<SubscriberDto>>
    {
        private static readonly TimeSpan[] DefaultRetrySleepDurations = {
            TimeSpan.FromSeconds(1.0),
            TimeSpan.FromSeconds(2.0),
        };

        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IDirectorServiceProxy _directorService;
        private readonly IEntityToDtoMapper _entityToDtoMapper;

        private readonly TimeSpan[] _retrySleepDurations;

        public DeleteSubscriberRequestHandler(
            ISubscriberRepository subscriberRepository,
            IDirectorServiceProxy directorService, IEntityToDtoMapper entityToDtoMapper, TimeSpan[] sleepDurations = null)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _directorService = directorService ?? throw new ArgumentNullException(nameof(directorService));
            _entityToDtoMapper = entityToDtoMapper ?? throw new ArgumentNullException(nameof(entityToDtoMapper));
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

            return await _subscriberRepository.GetSubscriberAsync(subscriberId)
                .Then(async subscriber => (await _directorService.DeleteReaderAsync(subscriber)).Then<SubscriberEntity>(x => subscriber))
                .Then(async subscriber => (await _subscriberRepository.RemoveSubscriberAsync(subscriberId)).Then<SubscriberEntity>(x => subscriber))
                .Then<SubscriberEntity, SubscriberDto>(subscriber => _entityToDtoMapper.MapSubscriber(subscriber));
        }
    }
}