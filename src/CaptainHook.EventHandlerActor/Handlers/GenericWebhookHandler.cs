using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// Generic WebHookConfig Handler which executes the call to a webhook based on the supplied configuration
    /// </summary>
    public class GenericWebhookHandler : IHandler
    {
        protected readonly IBigBrother BigBrother;
        protected readonly IHttpClientBuilder HttpClientBuilder;
        protected readonly IRequestBuilder RequestBuilder;
        protected readonly IRequestLogger RequestLogger;
        protected readonly WebhookConfig WebhookConfig;

        public GenericWebhookHandler(
            IHttpClientBuilder httpClientBuilder,
            IRequestBuilder requestBuilder,
            IRequestLogger requestLogger,
            IBigBrother bigBrother,
            WebhookConfig webhookConfig)
        {
            BigBrother = bigBrother;
            RequestBuilder = requestBuilder;
            RequestLogger = requestLogger;
            WebhookConfig = webhookConfig;
            HttpClientBuilder = httpClientBuilder;
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
                var httpVerb = RequestBuilder.SelectHttpVerb(WebhookConfig, messageData.Payload);
                var payload = RequestBuilder.BuildPayload(this.WebhookConfig, messageData.Payload, metadata);
                var authenticationScheme = RequestBuilder.SelectAuthenticationScheme(WebhookConfig, messageData.Payload);
                var config = RequestBuilder.SelectWebhookConfig(WebhookConfig, messageData.Payload);

                var httpClient = await HttpClientBuilder.BuildAsync(config, authenticationScheme, messageData.CorrelationId, cancellationToken);

                var handler = new HttpFailureLogger(BigBrother, messageData, uri.AbsoluteUri, httpVerb);
                var response = await httpClient.ExecuteAsJsonReliably(httpVerb, uri, payload, handler, token: cancellationToken);

                await RequestLogger.LogAsync(httpClient, response, messageData, uri, httpVerb);
            }
            catch (Exception e)
            {
                BigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }
    }
}
