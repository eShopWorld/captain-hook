using System;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
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

        public async Task<OperationResult<bool>> ProvisionReaderAsync(SubscriberEntity subscriber)
        {
            throw new NotImplementedException();

            //var subscriberConfigs = await _entityToConfigurationMapper.MapSubscriber(subscriber);
            //var createReaderResult = await _directorService.ApplyReaderChange(subscriberConfigs.Single());

            //return createReaderResult switch
            //{
            //    ReaderChangeResult.Success => true,
            //    ReaderChangeResult.CreateFailed => new ReaderCreationError(subscriber),
            //    ReaderChangeResult.DeleteFailed => new ReaderUpdateError(subscriber),
            //    ReaderChangeResult.DirectorIsBusy => new DirectorServiceIsBusyError(),
            //    _ => new BusinessError("Director Service returned unknown result.")
            //};
        }
    }
}