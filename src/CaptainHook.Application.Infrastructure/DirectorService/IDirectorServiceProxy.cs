using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
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
        /// Requests DirectorService to create, update or delete reader service
        /// </summary>
        /// <param name="request">Request to be processed by DirectorService</param>
        /// <returns>
        /// A SubscriberConfiguration if creation has been invoked
        /// ReaderCreateError if reader creation failed
        /// ReaderAlreadyExistsError if reader can't be crated because it already exists
        /// DirectorServiceIsBusyError if DirectorService is performing another operation
        /// </returns>
        Task<OperationResult<SubscriberConfiguration>> CallDirectorServiceAsync(ReaderChangeBase request);
    }
}