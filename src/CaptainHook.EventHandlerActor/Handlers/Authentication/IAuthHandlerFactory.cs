using System;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public interface IAuthHandlerFactory
    {
        /// <summary>
        /// Gets the token provider based on key
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IAcquireTokenHandler> GetAsync(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the token provider based on host
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IAcquireTokenHandler> GetAsync(Uri uri, CancellationToken cancellationToken);
    }
}
