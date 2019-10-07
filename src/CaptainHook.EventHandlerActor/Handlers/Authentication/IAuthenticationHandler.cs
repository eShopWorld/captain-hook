using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public interface IAuthenticationHandler
    {
        /// <summary>
        /// Gets a token from the STS based on the supplied credentials and scopes using the client grant OIDC 2 Flow
        /// This method also does token renewal based on requesting a token if the token is set to expire in the next ten seconds.
        /// </summary>
        /// <returns></returns>
        Task<string> GetTokenAsync(CancellationToken cancellationToken);
    }
}
