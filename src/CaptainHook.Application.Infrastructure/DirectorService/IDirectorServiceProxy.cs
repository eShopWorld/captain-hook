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
        /// Creates reader service for single webhook in Subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be created</param>
        /// <returns>
        /// True if creation has been invoked
        /// False if reader already exists
        /// ReaderCreationError if reader creation failed
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// </returns>
        Task<OperationResult<bool>> CreateReaderAsync(SubscriberEntity subscriber);

        /// <summary>
        /// Updates reader service for the given subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be updated</param>
        /// <returns>
        /// True if update has been invoked
        /// False if reader does not exist
        /// ReaderUpdateError if reader update failed
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// </returns>
        Task<OperationResult<bool>> UpdateReaderAsync(SubscriberEntity subscriber);
    }
}