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
            var subscriberConfigs = await _entityToConfigurationMapper.MapSubscriber(subscriber);
            var createReaderResult = await _directorService.ProvisionReaderAsync(subscriberConfigs.Single());

            switch (createReaderResult)
            {
                case ReaderProvisionResult.Created:
                case ReaderProvisionResult.Updated:
                case ReaderProvisionResult.ReaderAlreadyExists:
                    return true;
                case ReaderProvisionResult.CreateFailed:
                case ReaderProvisionResult.UpdateFailed:
                    return new ReaderCreationError(subscriber);
                case ReaderProvisionResult.DeleteFailed:
                    return new ReaderDeletionError(subscriber);
                case ReaderProvisionResult.DirectorIsBusy:
                    return new DirectorServiceIsBusyError();
                default:
                    return new BusinessError("Director Service returned unknown result.");
            }
        }
    }
}