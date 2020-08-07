using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface ISubscriberConfigurationLoader
    {
        Task<IEnumerable<SubscriberConfiguration>> LoadAsync(string keyVaultUri);
    }
}