using System;
using System.Linq;
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

        public async Task<OperationResult<bool>> CreateReaderAsync(SubscriberEntity subscriber)
        {
            return await CallDirector(s => new CreateReader {Subscriber = s}, subscriber);
        }

        public async Task<OperationResult<bool>> UpdateReaderAsync(SubscriberEntity subscriber)
        {
            return await CallDirector(s => new UpdateReader {Subscriber = s}, subscriber);
        }

        private async Task<OperationResult<bool>> CallDirector(Func<SubscriberConfiguration, ReaderChangeBase> requestFunc, SubscriberEntity subscriber)
        {
            var subscriberConfigsResult = await _entityToConfigurationMapper.MapSubscriberAsync(subscriber);

            if(subscriberConfigsResult.IsError)
            {
                return subscriberConfigsResult.Error;
            }

            var singleSubscriber = subscriberConfigsResult.Data.Single();

            var request = requestFunc(singleSubscriber);
            var createReaderResult = await _directorService.ApplyReaderChange(request);

            return createReaderResult switch
            {
                ReaderChangeResult.Success => true,
                ReaderChangeResult.NoChangeNeeded => true,
                ReaderChangeResult.CreateFailed => new ReaderCreateError(subscriber),
                ReaderChangeResult.DeleteFailed => new ReaderDeleteError(subscriber),
                ReaderChangeResult.DirectorIsBusy => new DirectorServiceIsBusyError(),
                ReaderChangeResult.ReaderAlreadyExist => new ReaderAlreadyExistsError(subscriber),
                ReaderChangeResult.ReaderDoesNotExist => new ReaderDoesNotExistError(subscriber),
                _ => new BusinessError("Director Service returned unknown result.")
            };
        }
    }
}