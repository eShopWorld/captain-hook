using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IExtendedHttpClientFactory _httpClientFactory;
        protected readonly IRequestBuilder RequestBuilder;
        protected readonly WebhookConfig WebhookConfig;
        private readonly IAuthenticationHandlerFactory _authenticationHandlerFactory;

        public GenericWebhookHandler(
            IAuthenticationHandlerFactory authenticationHandlerFactory,
            IRequestBuilder requestBuilder,
            IBigBrother bigBrother,
            IExtendedHttpClientFactory httpClientFactory,
            WebhookConfig webhookConfig)
        {
            BigBrother = bigBrother;
            _httpClientFactory = httpClientFactory;
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

                var httpClient = await GetHttpClient(cancellationToken, config, authenticationScheme);

                void TelemetryEvent(string msg)
                {
                    BigBrother.Publish(new HttpClientFailure(messageData.Handle, messageData.Type, payload, msg));
                }

                var response = await httpClient.ExecuteAsJsonReliably(httpVerb, uri, payload, TelemetryEvent, token: cancellationToken);
                
                BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, $"Response status code {response.StatusCode}"));
            }
            catch (Exception e)
            {
                BigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }

        /// <summary>
        /// Gets a configured http client for use in a request from the http client factory
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="config"></param>
        /// <param name="authenticationScheme"></param>
        /// <returns></returns>
        protected virtual async Task<HttpClient> GetHttpClient(CancellationToken cancellationToken, WebhookConfig config, AuthenticationType authenticationScheme)
        {
            var httpClient = _httpClientFactory.CreateClient(new Uri(config.Uri).Host, config);

            if (authenticationScheme == AuthenticationType.None)
            {
                return httpClient;
            }

            var acquireTokenHandler = await _authenticationHandlerFactory.GetAsync(config.Uri, cancellationToken);
            await acquireTokenHandler.GetTokenAsync(httpClient, cancellationToken);

            return httpClient;
        }
    }
}
