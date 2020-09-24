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

        public async Task<OperationResult<SubscriberConfiguration>> CreateReaderAsync(SubscriberEntity subscriber)
        {
             return await _entityToConfigurationMapper.MapToWebhookAsync(subscriber)
                   .Then(async sc => await CallDirector(new CreateReader { Subscriber = sc }));
        }

        public async Task<OperationResult<SubscriberConfiguration>> UpdateReaderAsync(SubscriberEntity subscriber)
        {
            return await _entityToConfigurationMapper.MapToWebhookAsync(subscriber)
                   .Then(async sc => await CallDirector(new UpdateReader { Subscriber = sc }));
        }

        public async Task<OperationResult<SubscriberConfiguration>> DeleteReaderAsync(SubscriberEntity subscriber)
        {
            return await _entityToConfigurationMapper.MapToWebhookAsync(subscriber)
                   .Then(async sc => await CallDirector(new DeleteReader { Subscriber = sc }));
        }

        public async Task<OperationResult<SubscriberConfiguration>> CreateDlqReaderAsync(SubscriberEntity subscriber)
        {
            return await _entityToConfigurationMapper.MapToDlqAsync(subscriber)
                   .Then(async sc => await CallDirector(new CreateReader { Subscriber = sc }));
        }

        public async Task<OperationResult<SubscriberConfiguration>> UpdateDlqReaderAsync(SubscriberEntity subscriber)
        {
            return await _entityToConfigurationMapper.MapToDlqAsync(subscriber)
                   .Then(async sc => await CallDirector(new UpdateReader { Subscriber = sc }));
        }

        public async Task<OperationResult<SubscriberConfiguration>> DeleteDlqReaderAsync(SubscriberEntity subscriber)
        {
            return await _entityToConfigurationMapper.MapToDlqAsync(subscriber)
                   .Then(async sc => await CallDirector(new DeleteReader { Subscriber = sc }));
        }

        private async Task<OperationResult<SubscriberConfiguration>> CallDirector(ReaderChangeBase request)
        {
            var createReaderResult = await _directorService.ApplyReaderChange(request);

            return createReaderResult switch
            {
                ReaderChangeResult.Success => request.Subscriber,
                ReaderChangeResult.NoChangeNeeded => request.Subscriber,
                ReaderChangeResult.CreateFailed => new ReaderCreateError(request.Subscriber.EventType, request.Subscriber.SubscriberName),
                ReaderChangeResult.DeleteFailed => new ReaderDeleteError(request.Subscriber.EventType, request.Subscriber.SubscriberName),
                ReaderChangeResult.DirectorIsBusy => new DirectorServiceIsBusyError(),
                ReaderChangeResult.ReaderAlreadyExist => new ReaderAlreadyExistsError(request.Subscriber.EventType, request.Subscriber.SubscriberName),
                ReaderChangeResult.ReaderDoesNotExist => new ReaderDoesNotExistError(request.Subscriber.EventType, request.Subscriber.SubscriberName),
                _ => new BusinessError("Director Service returned unknown result.")
            };
        }
    }
}