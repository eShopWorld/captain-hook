using System;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace CaptainHook.Application.Infrastructure.DirectorService
{
    public class DirectorServiceGateway : IDirectorServiceGateway
    {
        private readonly ISubscriberEntityToConfigurationMapper _entityToConfigurationMapper;

        public DirectorServiceGateway(ISubscriberEntityToConfigurationMapper entityToConfigurationMapper)
        {
            _entityToConfigurationMapper = entityToConfigurationMapper;
        }

        public async Task<OperationResult<SubscriberEntity>> CreateReaderAsync(SubscriberEntity subscriber)
        {
            var subscriberConfigs = await _entityToConfigurationMapper.MapSubscriber(subscriber);

            var directorServiceUri = new Uri(ServiceNaming.DirectorServiceFullName);
            var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(directorServiceUri);
            var createReaderResult = await directorServiceClient.CreateReaderAsync(subscriberConfigs.Single());

            switch (createReaderResult)
            {
                case CreateReaderResult.Created:
                    return true;
                case CreateReaderResult.DirectorIsBusy:
                    return new DirectorServiceIsBusyError();
                case CreateReaderResult.Failed:
                    return new ReaderCreationError(subscriber);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}