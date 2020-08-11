using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    /// <summary>
    /// Basic interface which allows to communicate Application with DirectorService.
    /// </summary>
    /// <remarks>
    /// Data passed through remoting must be serializable, so we must not use entities and OperationResult here.
    /// That's why SubscriberConfiguration is passed and enums are received.
    /// </remarks>
    public interface IDirectorServiceRemoting : IService
    {
        Task<RequestReloadConfigurationResult> RequestReloadConfigurationAsync();
        Task<IDictionary<string, SubscriberConfiguration>> GetAllSubscribersAsync();
        Task<ReaderProvisionResult> ProvisionReaderAsync(SubscriberConfiguration subscriber);
    }
}