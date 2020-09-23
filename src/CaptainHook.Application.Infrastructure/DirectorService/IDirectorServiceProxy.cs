using System.Collections.Generic;
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
        /// Creates reader service for single webhook in Subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be created</param>
        /// <returns>
        /// A IEnumerable<SubscriberConfiguration> if creation has been invoked
        /// ReaderCreateError if reader creation failed
        /// ReaderAlreadyExistsError if reader can't be crated because it already exists
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// </returns>
        Task<OperationResult<IEnumerable<SubscriberConfiguration>>> CreateReaderAsync(SubscriberEntity subscriber);

        /// <summary>
        /// Updates reader service for the given subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be updated</param>
        /// <returns>
        /// A IEnumerable<SubscriberConfiguration> if update has been invoked
        /// ReaderCreateError if reader create failed
        /// ReaderDeleteError if reader delete failed
        /// ReaderAlreadyExistsError if reader can't be crated because it already exists
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// ReaderDoesNotExistError if reader can't be deleted because it doesn't exist
        /// </returns>
        Task<OperationResult<IEnumerable<SubscriberConfiguration>>> UpdateReaderAsync(SubscriberEntity subscriber);

        /// <summary>
        /// Deletes reader service for the given subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be deleted</param>
        /// <returns>
        /// A IEnumerable<SubscriberConfiguration> if deleted has been invoked
        /// ReaderDeleteError if reader delete failed
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// ReaderDoesNotExistError if reader can't be deleted because it doesn't exist
        /// </returns>
        Task<OperationResult<IEnumerable<SubscriberConfiguration>>> DeleteReaderAsync(SubscriberEntity subscriber);



        Task<OperationResult<SubscriberConfiguration>> CreateReaderAsync(SubscriberConfiguration subscriber);

        Task<OperationResult<SubscriberConfiguration>> UpdateReaderAsync(SubscriberConfiguration subscriber);

        Task<OperationResult<SubscriberConfiguration>> DeleteReaderAsync(SubscriberConfiguration subscriber);
    }
}