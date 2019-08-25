using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Authentication;

namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// Builds out a http client for the given request flow
    /// </summary>
    public class HttpClientBuilder : IHttpClientBuilder
    {
        private readonly IAuthenticationHandlerFactory _authenticationHandlerFactory;
        private readonly IIndex<string, HttpClient> _httpClients;

        public HttpClientBuilder(
            IAuthenticationHandlerFactory authenticationHandlerFactory, 
            IIndex<string, HttpClient> httpClients)
        {
            _authenticationHandlerFactory = authenticationHandlerFactory;
            _httpClients = httpClients;
        }

        /// <summary>
        /// Gets a configured http client for use in a request from the http client factory
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="config"></param>
        /// <param name="authenticationScheme"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public async Task<HttpClient> BuildAsync(WebhookConfig config, AuthenticationType authenticationScheme, string correlationId, CancellationToken cancellationToken)
        {
            var uri = new Uri(config.Uri);

            if (!_httpClients.TryGetValue(uri.Host, out var httpClient))
            {
                throw new ArgumentNullException(nameof(httpClient), $"HttpClient for {uri.Host} was not found");
            }

            httpClient.DefaultRequestHeaders.Remove(Constants.Headers.CorrelationId);
            httpClient.DefaultRequestHeaders.Add(Constants.Headers.CorrelationId, correlationId);

            if (authenticationScheme == AuthenticationType.None)
            {
                return httpClient;
            }

            var acquireTokenHandler = await _authenticationHandlerFactory.GetAsync(uri, cancellationToken);
            await acquireTokenHandler.GetTokenAsync(httpClient, cancellationToken);

            return httpClient;
        }
    }
}