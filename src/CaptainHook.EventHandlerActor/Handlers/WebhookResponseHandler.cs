using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class WebhookResponseHandler : GenericWebhookHandler
    {
        private readonly IEventHandlerFactory _eventHandlerFactory;
        private readonly IRequestLogger _requestLogger;

        public WebhookResponseHandler(
            IEventHandlerFactory eventHandlerFactory,
            IHttpClientFactory httpClientFactory,
            IRequestBuilder requestBuilder,
            IAuthenticationHandlerFactory authenticationHandlerFactory,
            IRequestLogger requestLogger,
            IBigBrother bigBrother,
            SubscriberConfiguration subscriberConfiguration)
            : base(httpClientFactory, authenticationHandlerFactory, requestBuilder, requestLogger, bigBrother, subscriberConfiguration)
        {
            _eventHandlerFactory = eventHandlerFactory;
            _requestLogger = requestLogger;
        }

        public override async Task<bool> CallAsync<TRequest>(TRequest request, IDictionary<string, object> metadata, CancellationToken cancellationToken)
        {
            if (!(request is MessageData messageData))
            {
                throw new Exception("injected wrong implementation");
            }

            var uri = RequestBuilder.BuildUri(WebhookConfig, messageData.Payload);
            if (uri == null)
            {
                return true; //consider successful delivery
            }
            var httpMethod = RequestBuilder.SelectHttpMethod(WebhookConfig, messageData.Payload);
            var payload = RequestBuilder.BuildPayload(this.WebhookConfig, messageData.Payload, metadata);
            var config = RequestBuilder.SelectWebhookConfig(WebhookConfig, messageData.Payload);
            var headers = RequestBuilder.GetHttpHeaders(WebhookConfig, messageData);
            var authenticationConfig = RequestBuilder.GetAuthenticationConfig(WebhookConfig, messageData.Payload);

            var httpClient = HttpClientFactory.Get(config);

            await AddAuthenticationHeaderAsync(cancellationToken, authenticationConfig, headers);

            var response = await httpClient.SendRequestReliablyAsync(httpMethod, uri, headers, payload, cancellationToken);

            await _requestLogger.LogAsync(httpClient, response, messageData, uri, httpMethod, headers);

            //do not proceed to callback if response indicates "delivery failure"
            if (response.IsDeliveryFailure())
            {
                return false;
            }

            if (metadata == null)
            {
                metadata = new Dictionary<string, object>();
            }
            else
            {
                metadata.Clear();
            }

            var content = await response.Content.ReadAsStringAsync();
            metadata.Add("HttpStatusCode", (int)response.StatusCode);
            metadata.Add("HttpResponseContent", content);

            //call callback
            var eswHandler = _eventHandlerFactory.CreateWebhookHandler(messageData.SubscriberConfig?.Callback);

            return await eswHandler.CallAsync(messageData, metadata, cancellationToken);
        }
    }
}
