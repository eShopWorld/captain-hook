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

        public async Task<OperationResult<IEnumerable<SubscriberEntity>>> CreateReaderAsync(SubscriberEntity subscriber)
        {
            return await CallDirector(s => new CreateReader { Subscriber = s }, subscriber);
        }

        public async Task<OperationResult<IEnumerable<SubscriberEntity>>> UpdateReaderAsync(SubscriberEntity subscriber)
        {
            return await CallDirector(s => new UpdateReader { Subscriber = s }, subscriber);
        }

        public async Task<OperationResult<IEnumerable<SubscriberEntity>>> DeleteReaderAsync(SubscriberEntity subscriber)
        {
            return await CallDirector(s => new DeleteReader { Subscriber = s }, subscriber);
        }

        private async Task<OperationResult<IEnumerable<SubscriberEntity>>> CallDirector(Func<SubscriberConfiguration, ReaderChangeBase> requestFunc, SubscriberEntity subscriber)
        {
            var subscriberConfigsResult = await _entityToConfigurationMapper.MapSubscriberAsync(subscriber);

            if(subscriberConfigsResult.IsError)
            {
                return subscriberConfigsResult.Error;
            }

            var subscriberConfigs = new List<SubscriberEntity>();

            foreach (var subscriberConfig in subscriberConfigsResult.Data)
            {
                var request = requestFunc(subscriberConfig);
                var createReaderResult = await _directorService.ApplyReaderChange(request);

                OperationResult<SubscriberEntity> operationResult = createReaderResult switch
                {
                    ReaderChangeResult.Success => subscriber,
                    ReaderChangeResult.NoChangeNeeded => subscriber,
                    ReaderChangeResult.CreateFailed => new ReaderCreateError(subscriber),
                    ReaderChangeResult.DeleteFailed => new ReaderDeleteError(subscriber),
                    ReaderChangeResult.DirectorIsBusy => new DirectorServiceIsBusyError(),
                    ReaderChangeResult.ReaderAlreadyExist => new ReaderAlreadyExistsError(subscriber),
                    ReaderChangeResult.ReaderDoesNotExist => new ReaderDoesNotExistError(subscriber),
                    _ => new BusinessError("Director Service returned unknown result.")
                };

                if(operationResult.IsError)
                {
                    return operationResult.Error;
                }

                subscriberConfigs.Add(operationResult.Data);
            }

            return subscriberConfigs;
        }
    }
}