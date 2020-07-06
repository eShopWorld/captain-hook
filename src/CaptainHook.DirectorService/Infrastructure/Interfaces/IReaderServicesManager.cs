using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface IReaderServicesManager
    {
        /// <summary>
        /// Compares newly read Configuration with list of currently deployed subscribers and based on that create new, delete old
        /// and refresh (by pair of create and delete operation) existing readers.
        /// </summary>
        /// <param name="subscribers">Target Configuration to be deployed</param>
        /// <param name="deployedServicesNames">List of currently deployed services names</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns></returns>
        Task RefreshReadersAsync(IEnumerable<SubscriberConfiguration> subscribers, IEnumerable<string> deployedServicesNames, bool inRelease, CancellationToken cancellationToken);
    }
}