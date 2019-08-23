using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// Generic WebHookConfig Handler which executes the call to a webhook based on the supplied configuration
    /// </summary>
    public class GenericWebhookHandler : IHandler
    {
        protected readonly IBigBrother BigBrother;
        private readonly IIndex<string, HttpClient> _httpClients;
        protected readonly IRequestBuilder RequestBuilder;
        protected readonly WebhookConfig WebhookConfig;
        private readonly IAuthenticationHandlerFactory _authenticationHandlerFactory;

        public GenericWebhookHandler(
            IAuthenticationHandlerFactory authenticationHandlerFactory,
            IRequestBuilder requestBuilder,
            IBigBrother bigBrother,
            IIndex<string, HttpClient> httpClients,
            WebhookConfig webhookConfig)
        {
            BigBrother = bigBrother;
            _httpClients = httpClients;
            RequestBuilder = requestBuilder;
            WebhookConfig = webhookConfig;
            this._authenticationHandlerFactory = authenticationHandlerFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="metadata"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task CallAsync<TRequest>(TRequest request, IDictionary<string, object> metadata, CancellationToken cancellationToken)
        {
            try
            {
                if (!(request is MessageData messageData))
                {
                    throw new Exception("injected wrong implementation");
                }

                var uri = RequestBuilder.BuildUri(WebhookConfig, messageData.Payload);
                var httpVerb = RequestBuilder.SelectHttpVerb(WebhookConfig, messageData.Payload);
                var payload = RequestBuilder.BuildPayload(this.WebhookConfig, messageData.Payload, metadata);
                var authenticationScheme = RequestBuilder.SelectAuthenticationScheme(WebhookConfig, messageData.Payload);
                var config = RequestBuilder.SelectWebhookConfig(WebhookConfig, messageData.Payload);

                var httpClient = await GetHttpClient(cancellationToken, config, authenticationScheme, messageData.CorrelationId);

                var handler = new HttpFailureLogger(BigBrother, messageData, uri.AbsoluteUri, httpVerb);
                var response = await httpClient.ExecuteAsJsonReliably(httpVerb, uri, payload, handler, token: cancellationToken);

                await LogEventAsync(httpClient.DefaultRequestHeaders.ToString(), messageData, response, uri, httpVerb);

            }
            catch (Exception e)
            {
                BigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }

        private async Task LogEventAsync(string headers, 
            MessageData messageData,
            HttpResponseMessage response, 
            Uri uri, 
            HttpVerb httpVerb)
        {
            BigBrother.Publish(new WebhookEvent(
                messageData.Handle,
                messageData.Type,
                $"Response status code {response.StatusCode}",
                uri.AbsoluteUri,
                httpVerb,
                response.StatusCode,
                messageData.CorrelationId));

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            BigBrother.Publish(new FailedWebHookEvent(
                headers,
                messageData.Payload,
                await response.Content.ReadAsStringAsync(),
                messageData.Handle,
                messageData.Type,
                $"Response status code {response.StatusCode}",
                uri.AbsoluteUri,
                httpVerb,
                response.StatusCode,
                messageData.CorrelationId));
        }

        /// <summary>
        /// Gets a configured http client for use in a request from the http client factory
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="config"></param>
        /// <param name="authenticationScheme"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        protected async Task<HttpClient> GetHttpClient(CancellationToken cancellationToken, WebhookConfig config, AuthenticationType authenticationScheme, string correlationId)
        {
            var uri = new Uri(config.Uri);

            if (!_httpClients.TryGetValue(uri.Host, out var httpClient))
            {
                throw new ArgumentNullException(nameof(httpClient), $"HttpClient for {uri.Host} was not found");
            }

            if (authenticationScheme == AuthenticationType.None)
            {
                return httpClient;
            }

            var acquireTokenHandler = await _authenticationHandlerFactory.GetAsync(uri, cancellationToken);
            await acquireTokenHandler.GetTokenAsync(httpClient, cancellationToken);

            httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

            return httpClient;
        }
    }
}
