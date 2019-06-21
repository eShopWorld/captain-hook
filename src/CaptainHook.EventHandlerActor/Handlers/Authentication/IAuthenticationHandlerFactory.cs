using System;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public interface IAuthenticationHandlerFactory
    {
        /// <summary>
        /// Gets the token provider based on key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IAcquireTokenHandler> GetAsync(string key, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the token provider based on host
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IAcquireTokenHandler> GetAsync(Uri uri, CancellationToken cancellationToken);
    }
}
