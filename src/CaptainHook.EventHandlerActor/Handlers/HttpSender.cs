using CaptainHook.Common;
using IdentityModel.Client;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Polly;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class HttpSender : IHttpSender
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpSender(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException();
        }

        public async Task<TokenResponse> RequestClientCredentialsTokenAsync(ClientCredentialsTokenRequest request, CancellationToken cancellationToken = default)
        {
            if(request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!Uri.TryCreate(request.Address, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException(nameof(request.Address));
            }

            var httpClient = _httpClientFactory.Get(uri);

            return await httpClient.RequestClientCredentialsTokenAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> SendAsync(SendRequest sendRequest, CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.Get(sendRequest.Uri, sendRequest.Timeout);

            return await RetryRequest(() =>
            {
                var request = new HttpRequestMessage(sendRequest.HttpMethod, sendRequest.Uri);

                if (sendRequest.HttpMethod != HttpMethod.Get)
                {
                    request.Content = new StringContent(sendRequest.Payload, Encoding.UTF8, sendRequest.Headers.ContentHeaders[Constants.Headers.ContentType]);
                }

                foreach (var key in sendRequest.Headers.RequestHeaders.Keys)
                {
                    //todo is this the correct thing to do when there is a CorrelationVector with multiple Children.
                    if (request.Headers.Contains(key))
                    {
                        request.Headers.Remove(key);
                    }

                    request.Headers.Add(key, sendRequest.Headers.RequestHeaders[key]);
                }

                return httpClient.SendAsync(request, cancellationToken);
            });

        }

        /// <summary>
        /// Executes the supplied func with reties and reports on it if something goes wrong ideally to BigBrother
        /// </summary>
        /// <param name="makeTheCall"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> RetryRequest(
            Func<Task<HttpResponseMessage>> makeTheCall)
        {
            //todo the retry status codes need to be customisable from the webhook config api
            return await Policy
                .HandleResult<HttpResponseMessage>(message => message.IsDeliveryFailure())
                .WaitAndRetryAsync(new[]
                {
                    //todo config this + jitter
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(30)
                }).ExecuteAsync(makeTheCall.Invoke);
        }
    }
}
