using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
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
        /// Creates reader service related to webhook in given subscriber entity.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be created</param>
        /// <returns>
        /// A SubscriberConfiguration if creation has been invoked
        /// ReaderCreateError if reader creation failed
        /// ReaderAlreadyExistsError if reader can't be crated because it already exists
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// </returns>
        Task<OperationResult<SubscriberConfiguration>> CreateReaderAsync(SubscriberEntity subscriber);

        /// <summary>
        /// Updates reader service related to webhook in given subscriber entity.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be updated</param>
        /// <returns>
        /// A SubscriberConfiguration if creation has been invoked
        /// ReaderCreateError if reader create failed
        /// ReaderDeleteError if reader delete failed
        /// ReaderAlreadyExistsError if reader can't be crated because it already exists
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// ReaderDoesNotExistError if reader can't be deleted because it doesn't exist
        /// </returns>
        Task<OperationResult<SubscriberConfiguration>> UpdateReaderAsync(SubscriberEntity subscriber);

        /// <summary>
        /// Deletes reader service related to webhook in given subscriber entity.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be deleted</param>
        /// <returns>
        /// A SubscriberConfiguration if creation has been invoked
        /// ReaderDeleteError if reader delete failed
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// ReaderDoesNotExistError if reader can't be deleted because it doesn't exist
        /// </returns>
        Task<OperationResult<SubscriberConfiguration>> DeleteReaderAsync(SubscriberEntity subscriber);

        /// <summary>
        /// Creates reader service related to DLQ in given subscriber entity.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be created</param>
        /// <returns>
        /// A SubscriberConfiguration if creation has been invoked
        /// ReaderCreateError if reader creation failed
        /// ReaderAlreadyExistsError if reader can't be crated because it already exists
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// </returns>
        Task<OperationResult<SubscriberConfiguration>> CreateDlqReaderAsync(SubscriberEntity subscriber);

        /// <summary>
        /// Updates reader service related to DLQ in given subscriber entity.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be updated</param>
        /// <returns>
        /// A SubscriberConfiguration if creation has been invoked
        /// ReaderCreateError if reader create failed
        /// ReaderDeleteError if reader delete failed
        /// ReaderAlreadyExistsError if reader can't be crated because it already exists
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// ReaderDoesNotExistError if reader can't be deleted because it doesn't exist
        /// </returns>
        Task<OperationResult<SubscriberConfiguration>> UpdateDlqReaderAsync(SubscriberEntity subscriber);

        /// <summary>
        /// Deletes reader service related to DLQ in given subscriber entity.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be deleted</param>
        /// <returns>
        /// A SubscriberConfiguration if creation has been invoked
        /// ReaderDeleteError if reader delete failed
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// ReaderDoesNotExistError if reader can't be deleted because it doesn't exist
        /// </returns>
        Task<OperationResult<SubscriberConfiguration>> DeleteDlqReaderAsync(SubscriberEntity subscriber);
    }
}