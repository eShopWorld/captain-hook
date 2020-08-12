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
            var subscriberConfigs = await _entityToConfigurationMapper.MapSubscriberAsync(subscriber);
            var createReaderResult = await _directorService.CreateReaderAsync(subscriberConfigs.Single());

            return createReaderResult switch
            {
                CreateReaderResult.Created => true,
                CreateReaderResult.AlreadyExists => true,
                CreateReaderResult.DirectorIsBusy => new DirectorServiceIsBusyError(),
                CreateReaderResult.Failed => new ReaderCreationError(subscriber),
                _ => new BusinessError("Director Service returned unknown result.")
            };
        }

        public Task<OperationResult<bool>> UpdateReaderAsync(SubscriberEntity subscriber)
        {
            return Task.FromResult(new OperationResult<bool>(true));
        }
    }
}