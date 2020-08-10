using System.Threading.Tasks;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Infrastructure.DirectorService
{
    /// <summary>
    /// Proxy to Director Service
    /// </summary>
    public interface IDirectorServiceProxy
    {
        /// <summary>
        /// Creates or updates single reader service for the given subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be updated</param>
        /// <returns>
        /// True if update has been invoked
        /// False if reader does not exist
        /// ReaderCreationError if reader create failed for new or update
        /// ReaderDeletionError if reader delete failed
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// </returns>
        Task<OperationResult<bool>> RefreshReaderAsync(SubscriberEntity subscriber);
    }
}