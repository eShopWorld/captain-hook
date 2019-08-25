using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IHttpClientBuilder
    {
        Task<HttpClient> BuildAsync(
            WebhookConfig config,
            AuthenticationType authenticationScheme,
            string correlationId,
            CancellationToken cancellationToken);
    }
}