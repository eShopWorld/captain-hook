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

        public async Task<OperationResult<bool>> CreateReaderAsync(SubscriberEntity subscriber)
        {
            var subscriberConfigs = await _entityToConfigurationMapper.MapSubscriber(subscriber);
            var createReaderResult = await _directorService.RefreshReaderAsync(subscriberConfigs.Single());

            return createReaderResult switch
            {
                ReaderRefreshResult.Created => true,
                ReaderRefreshResult.Updated => true,
                ReaderRefreshResult.ReaderAlreadyExists => true,
                ReaderRefreshResult.CreateFailed => new ReaderCreationError(subscriber),
                ReaderRefreshResult.UpdateFailed => new ReaderCreationError(subscriber),
                ReaderRefreshResult.DeleteFailed => new ReaderDeletionError(subscriber),
                ReaderRefreshResult.DirectorIsBusy => new DirectorServiceIsBusyError(),
                _ => new BusinessError("Director Service returned unknown result.")
            };
        }
    }
}