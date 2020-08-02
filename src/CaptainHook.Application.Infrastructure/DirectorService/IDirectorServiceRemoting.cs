using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Results;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CaptainHook.Application.Infrastructure.DirectorService
{
    public interface IDirectorServiceRemoting : IService
    {
        Task<RequestReloadConfigurationResult> RequestReloadConfigurationAsync();
        Task<IDictionary<string, SubscriberConfiguration>> GetAllSubscribersAsync();
        Task<OperationResult<bool>> CreateReaderAsync(SubscriberConfiguration subscriber);
    }
}
