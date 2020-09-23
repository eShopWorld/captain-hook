using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Infrastructure.DirectorService
{
    public class DirectorServiceProxy : IDirectorServiceProxy
    {
        private readonly ISubscriberEntityToConfigurationMapper _entityToConfigurationMapper;
        private readonly IDirectorServiceRemoting _directorService;

        public DirectorServiceProxy(ISubscriberEntityToConfigurationMapper entityToConfigurationMapper, IDirectorServiceRemoting directorService)
        {
            _entityToConfigurationMapper = entityToConfigurationMapper;
            _directorService = directorService;
        }

        public async Task<OperationResult<IEnumerable<SubscriberConfiguration>>> CreateReaderAsync(SubscriberEntity subscriber)
        {
            return await CallDirector(s => new CreateReader { Subscriber = s }, subscriber);
        }

        public async Task<OperationResult<IEnumerable<SubscriberConfiguration>>> UpdateReaderAsync(SubscriberEntity subscriber)
        {
            return await CallDirector(s => new UpdateReader { Subscriber = s }, subscriber);
        }

        public async Task<OperationResult<IEnumerable<SubscriberConfiguration>>> DeleteReaderAsync(SubscriberEntity subscriber)
        {
            return await CallDirector(s => new DeleteReader { Subscriber = s }, subscriber);
        }

        private async Task<OperationResult<IEnumerable<SubscriberConfiguration>>> CallDirector(
            Func<SubscriberConfiguration, ReaderChangeBase> requestFunc, SubscriberEntity subscriber)
        {
            var subscriberConfigsResult = await _entityToConfigurationMapper.MapSubscriberAsync(subscriber);

            if (subscriberConfigsResult.IsError)
            {
                return subscriberConfigsResult.Error;
            }

            foreach (var subscriberConfig in subscriberConfigsResult.Data)
            {
                var request = requestFunc(subscriberConfig);
                var createReaderResult = await _directorService.ApplyReaderChange(request);

                OperationResult<SubscriberConfiguration> operationResult = createReaderResult switch
                {
                    ReaderChangeResult.Success => subscriberConfig,
                    ReaderChangeResult.NoChangeNeeded => subscriberConfig,
                    ReaderChangeResult.CreateFailed => new ReaderCreateError(subscriber),
                    ReaderChangeResult.DeleteFailed => new ReaderDeleteError(subscriber),
                    ReaderChangeResult.DirectorIsBusy => new DirectorServiceIsBusyError(),
                    ReaderChangeResult.ReaderAlreadyExist => new ReaderAlreadyExistsError(subscriber),
                    ReaderChangeResult.ReaderDoesNotExist => new ReaderDoesNotExistError(subscriber),
                    _ => new BusinessError("Director Service returned unknown result.")
                };

                if (operationResult.IsError)
                {
                    return operationResult.Error;
                }
            }

            return subscriberConfigsResult;
        }

        public async Task<OperationResult<SubscriberConfiguration>> CreateReaderAsync(SubscriberConfiguration subscriber)
        {
            return await CallDirector(new CreateReader { Subscriber = subscriber });
        }

        public async Task<OperationResult<SubscriberConfiguration>> UpdateReaderAsync(SubscriberConfiguration subscriber)
        {
            return await CallDirector(new UpdateReader { Subscriber = subscriber });
        }

        public async Task<OperationResult<SubscriberConfiguration>> DeleteReaderAsync(SubscriberConfiguration subscriber)
        {
            return await CallDirector(new DeleteReader { Subscriber = subscriber });
        }

        private async Task<OperationResult<SubscriberConfiguration>> CallDirector(ReaderChangeBase request)
        {
            var subscriber = request.Subscriber;

            var createReaderResult = await _directorService.ApplyReaderChange(request);

            return createReaderResult switch
            {
                ReaderChangeResult.Success => request.Subscriber,
                ReaderChangeResult.NoChangeNeeded => request.Subscriber,
                ReaderChangeResult.CreateFailed => new ReaderCreateError(subscriber.EventType, subscriber.SubscriberName),
                ReaderChangeResult.DeleteFailed => new ReaderDeleteError(subscriber.EventType, subscriber.SubscriberName),
                ReaderChangeResult.DirectorIsBusy => new DirectorServiceIsBusyError(),
                ReaderChangeResult.ReaderAlreadyExist => new ReaderAlreadyExistsError(subscriber.EventType, subscriber.SubscriberName),
                ReaderChangeResult.ReaderDoesNotExist => new ReaderDoesNotExistError(subscriber.EventType, subscriber.SubscriberName),
                _ => new BusinessError("Director Service returned unknown result.")
            };
        }
    }
}