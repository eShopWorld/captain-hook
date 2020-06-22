using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CaptainHook.Common.Remoting
{
    public interface IDirectorServiceRemoting: IService
    {
        Task<RequestReloadConfigurationResult> RequestReloadConfigurationAsync();
        Task<IDictionary<string, SubscriberConfiguration>> GetAllSubscribers();
    }
}