using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using Kusto.Cloud.Platform.Utils;
using MediatR;
using Polly;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class DeleteSubscriberRequestHandler : IRequestHandler<DeleteSubscriberRequest, OperationResult<bool>>
    {
        private static readonly TimeSpan[] DefaultRetrySleepDurations = {
            TimeSpan.FromSeconds(1.0),
            TimeSpan.FromSeconds(2.0),
        };

        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IDirectorServiceProxy _directorService;
        private readonly TimeSpan[] _retrySleepDurations;

        public DeleteSubscriberRequestHandler(
            ISubscriberRepository subscriberRepository,
            IDirectorServiceProxy directorService,
            TimeSpan[] sleepDurations = null)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _directorService = directorService ?? throw new ArgumentNullException(nameof(directorService));
            _retrySleepDurations = sleepDurations?.SafeFastNullIfEmpty() ?? DefaultRetrySleepDurations;
        }

        public async Task<OperationResult<bool>> Handle(DeleteSubscriberRequest request, CancellationToken cancellationToken)
        {
            var executionResult = await Policy
                .HandleResult<OperationResult<bool>>(result => result.Error is CannotDeleteEntityError)
                .WaitAndRetryAsync(_retrySleepDurations)
                .ExecuteAsync(() => DeleteSubscriberAsync(request));

            return executionResult;
        }

        private async Task<OperationResult<bool>> DeleteSubscriberAsync(DeleteSubscriberRequest request)
        {
            var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
            var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

            if (existingItem.IsError)
            {
                return existingItem.Error;
            }

            var directorResult = await _directorService.DeleteReaderAsync(existingItem);
            if (directorResult.IsError)
            {
                return directorResult.Error;
            }

            var deleteResult = await _subscriberRepository.RemoveSubscriberAsync(subscriberId);
            if (deleteResult.IsError)
            {
                return deleteResult.Error;
            }

            return true;

            //var removeResult = existingItem.Data.RemoveWebhookEndpoint(EndpointEntity.FromSelector(request.Selector));
            //if (removeResult.IsError)
            //{
            //    return removeResult.Error;
            //}

            //var directorResult = await _directorService.UpdateReaderAsync(existingItem);
            //if (directorResult.IsError)
            //{
            //    return directorResult.Error;
            //}

            //var saveResult = await _subscriberRepository.UpdateSubscriberAsync(existingItem);
            //if (saveResult.IsError)
            //{
            //    return saveResult.Error;
            //}

            //return _entityToDtoMapper.MapSubscriber(saveResult);
        }
    }
}