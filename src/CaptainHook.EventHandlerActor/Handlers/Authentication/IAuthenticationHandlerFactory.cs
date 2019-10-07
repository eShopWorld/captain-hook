using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public interface IAuthenticationHandlerFactory
    {
        /// <summary>
        /// Gets the token provider based on host
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IAuthenticationHandler> GetAsync(WebhookConfig config, CancellationToken cancellationToken);
    }
}
