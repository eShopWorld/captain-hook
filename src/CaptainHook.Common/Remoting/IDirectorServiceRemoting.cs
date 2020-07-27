using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Remoting.Types;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CaptainHook.Common.Remoting
{
    public interface IDirectorServiceRemoting : IService
    {
        Task<RequestReloadConfigurationResult> RequestReloadConfigurationAsync();
        Task<IDictionary<string, SubscriberConfiguration>> GetAllSubscribersAsync();
        Task<RequestReloadConfigurationResult> UpdateReader(ReaderChangeInfo readerChangeInfo);
    }
}