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
        /// ReaderCreateError if reader creation failed
        /// ReaderAlreadyExistsError if reader can't be crated because it already exists
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
        /// ReaderCreateError if reader create failed
        /// ReaderDeleteError if reader delete failed
        /// ReaderAlreadyExistsError if reader can't be crated because it already exists
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// ReaderDoesNotExistError if reader can't be deleted because it doesn't exist
        /// </returns>
        Task<OperationResult<bool>> UpdateReaderAsync(SubscriberEntity subscriber);

        /// <summary>
        /// Deletes reader service for the given subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be deleted</param>
        /// <returns>
        /// True if deleted has been invoked
        /// False if reader does not exist
        /// ReaderDeleteError if reader delete failed
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// ReaderDoesNotExistError if reader can't be deleted because it doesn't exist
        /// </returns>
        Task<OperationResult<bool>> DeleteReaderAsync(SubscriberEntity subscriber);
    }
}