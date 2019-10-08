using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
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
        protected readonly IHttpClientFactory HttpClientFactory;
        protected readonly IRequestBuilder RequestBuilder;
        protected readonly WebhookConfig WebhookConfig;
        private readonly IRequestLogger _requestLogger;
        private readonly IAuthenticationHandlerFactory _authenticationHandlerFactory;

        public GenericWebhookHandler(
            IHttpClientFactory httpClientFactory,
            IAuthenticationHandlerFactory authenticationHandlerFactory,
            IRequestBuilder requestBuilder,
            IRequestLogger requestLogger,
            IBigBrother bigBrother,
            WebhookConfig webhookConfig)
        {
            BigBrother = bigBrother;
            RequestBuilder = requestBuilder;
            _requestLogger = requestLogger;
            WebhookConfig = webhookConfig;
            _authenticationHandlerFactory = authenticationHandlerFactory;
            HttpClientFactory = httpClientFactory;
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

                //todo refactor into a single call and a dto
                var uri = RequestBuilder.BuildUri(WebhookConfig, messageData.Payload);
                var httpMethod = RequestBuilder.SelectHttpMethod(WebhookConfig, messageData.Payload);
                var payload = RequestBuilder.BuildPayload(this.WebhookConfig, messageData.Payload, metadata);
                var config = RequestBuilder.SelectWebhookConfig(WebhookConfig, messageData.Payload);
                var headers = RequestBuilder.GetHttpHeaders(WebhookConfig, messageData);
                var authenticationConfig = RequestBuilder.GetAuthenticationConfig(WebhookConfig, messageData.Payload);

                var httpClient = HttpClientFactory.Get(config);
                
                await AddAuthenticationHeaderAsync(cancellationToken, authenticationConfig, headers);
                
                var response = await httpClient.SendRequestReliablyAsync(httpMethod, uri, headers, payload, cancellationToken);

                await _requestLogger.LogAsync(httpClient, response, messageData, uri, httpMethod);
            }
            catch (Exception e)
            {
                BigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }

        protected async Task AddAuthenticationHeaderAsync(CancellationToken cancellationToken, WebhookConfig config, WebHookHeaders webHookHeaders)
        {
            if (config.AuthenticationConfig.Type != AuthenticationType.None)
            {
                var acquireTokenHandler = await _authenticationHandlerFactory.GetAsync(config, cancellationToken);
                var result = await acquireTokenHandler.GetTokenAsync(cancellationToken);
                webHookHeaders.AddRequestHeader(Constants.Headers.Authorization, result);
            }
        }
    }
}
