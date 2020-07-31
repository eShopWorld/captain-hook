using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Remoting;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CaptainHook.Application.Gateways
{
    public interface IDirectorServiceRemoting : IService
    {
        Task<RequestReloadConfigurationResult> RequestReloadConfigurationAsync();
        Task<IDictionary<string, SubscriberConfiguration>> GetAllSubscribersAsync();

         /// <summary>
        /// Creates reader service for single webhook in Subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be created</param>
        /// <returns>
        /// True if creation has been invoked
        /// False if reader already exists
        /// ReaderCreationError if reader creation failed
        /// DirectorServiceIsBusyError if DirectorService is performing other operation
        /// </returns>
        Task<OperationResult<bool>> CreateReaderAsync(SubscriberEntity subscriber);
    }

    internal class DirectorServiceGateway : IDirectorServiceGateway
    {
        public Task<OperationResult<bool>> CreateReaderAsync(SubscriberEntity subscriber)
        {
            throw new System.NotImplementedException();
        }
    }
}
