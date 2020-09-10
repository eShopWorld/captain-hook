using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IHttpSender
    {
        public Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod, Uri uri, WebHookHeaders headers, string payload, TimeSpan timeout = default, CancellationToken cancellationToken = default);

        public Task<TokenResponse> RequestClientCredentialsTokenAsync(ClientCredentialsTokenRequest request, CancellationToken cancellationToken = default);
    }
}
