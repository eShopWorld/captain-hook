using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService.ReaderServiceManagement
{
    public interface IReaderServicesManager
    {
        /// <summary>
        /// Compares newly read Configuration with list of currently deployed subscribers and based on that create new, delete old
        /// and refresh (by pair of create and delete operation) existing readers.
        /// </summary>
        /// <param name="changeSet">List of changes to be applied to the readers</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns></returns>
        Task<Dictionary<string, RefreshReaderResult>> RefreshReadersAsync(IEnumerable<ReaderChangeInfo> changeSet, CancellationToken cancellationToken);
    }
}