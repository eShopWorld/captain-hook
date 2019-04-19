using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public interface IAcquireTokenHandler
    {
        /// <summary>
        /// Gets a token from the STS based on the supplied credentials and scopes using the client grant OIDC 2 Flow
        /// This method also does token renewal based on requesting a token if the token is set to expire in the next ten seconds.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task GetTokenAsync(HttpClient client, CancellationToken token);
    }
}
