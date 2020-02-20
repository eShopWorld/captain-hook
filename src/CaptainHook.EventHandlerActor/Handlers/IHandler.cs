using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// request/response handler interface
    /// </summary>
    public interface IHandler
    {
        /// <summary>
        /// handle request
        /// </summary>
        /// <typeparam name="TRequest">type of incoming structure</typeparam>
        /// <param name="request">incoming data instance</param>
        /// <param name="metaData">header key value pair</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>true for operation success</returns>
        Task<bool> CallAsync<TRequest>(TRequest request, IDictionary<string, object> metaData, CancellationToken cancellationToken);
    }
}
