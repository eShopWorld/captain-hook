using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    public interface IDirectorServiceRemoting : IService
    {
        Task<RequestReloadConfigurationResult> RequestReloadConfigurationAsync();
        Task<IDictionary<string, SubscriberConfiguration>> GetAllSubscribersAsync();
        Task<CreateReaderResult> CreateReaderAsync(SubscriberConfiguration subscriber);
    }
}