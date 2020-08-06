using CaptainHook.Common.Configuration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface ISubscriberConfigurationLoader
    {
        Task<ReadOnlyCollection<SubscriberConfiguration>> LoadAsync(string keyVaultUri);
    }
}