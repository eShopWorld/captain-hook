using System;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class EventHandlerFactory : IEventHandlerFactory
    {
        private readonly IBigBrother _bigBrother;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthenticationHandlerFactory _authenticationHandlerFactory;
        private readonly IRequestLogger _requestLogger;
        private readonly IRequestBuilder _requestBuilder;

        public EventHandlerFactory(
            IBigBrother bigBrother,
            IHttpClientFactory httpClientFactory,
            IAuthenticationHandlerFactory authenticationHandlerFactory,
            IRequestLogger requestLogger,
            IRequestBuilder requestBuilder)
        {
            _bigBrother = bigBrother;
            _httpClientFactory = httpClientFactory;
            _requestLogger = requestLogger;
            _requestBuilder = requestBuilder;
            _authenticationHandlerFactory = authenticationHandlerFactory;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create the custom handler such that we get a mapping from the webhook to the handler selected
        /// </summary>
        /// <param name="eventType">type of event to process</param>
        /// <param name="subscriberName">name of the subscriber that received the event</param>
        /// <returns>handler instance</returns>
        public IHandler CreateEventHandler(MessageData messageData)
        {
            var subscriberConfig = messageData.SubscriberConfig;
            if (subscriberConfig == null)
            {
                throw new Exception($"Boom, handler event type '{messageData.Type}' was not found, cannot process the message");
            }

            if (subscriberConfig.Callback != null)
            {
                return new WebhookResponseHandler(
                    this,
                    _httpClientFactory,
                    _requestBuilder,
                    _authenticationHandlerFactory,
                    _requestLogger,
                    _bigBrother,
                    messageData.SubscriberConfig);
            }

            return CreateWebhookHandler(messageData.WebhookConfig, messageData.SubscriberConfig.Name);
        }

        /// <summary>
        /// Creates a single fire and forget webhook handler
        /// Need this here for now to select the handler for the callback
        /// </summary>
        /// <param name="webHookName"></param>
        /// <returns></returns>
        public IHandler CreateWebhookHandler(WebhookConfig webhookConfig, string webHookName = "")
        {
            if (webhookConfig == null)
            {
                throw new Exception("Boom, handler webhook not found cannot process the message");
            }

            return new GenericWebhookHandler(
                _httpClientFactory,
                _authenticationHandlerFactory,
                _requestBuilder,
                _requestLogger,
                _bigBrother,
                webhookConfig);
        }
    }
}
