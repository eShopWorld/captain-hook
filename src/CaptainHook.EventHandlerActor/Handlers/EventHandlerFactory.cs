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
        private readonly IHttpSender _httpSender;
        private readonly IAuthenticationHandlerFactory _authenticationHandlerFactory;
        private readonly IRequestLogger _requestLogger;
        private readonly IRequestBuilder _requestBuilder;

        public EventHandlerFactory(
            IBigBrother bigBrother,
            IHttpSender httpSender,
            IAuthenticationHandlerFactory authenticationHandlerFactory,
            IRequestLogger requestLogger,
            IRequestBuilder requestBuilder)
        {
            _bigBrother = bigBrother;
            _httpSender = httpSender;
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
                    _httpSender,
                    _requestBuilder,
                    _authenticationHandlerFactory,
                    _requestLogger,
                    _bigBrother,
                    messageData.SubscriberConfig);
            }

            return CreateWebhookHandler(messageData.SubscriberConfig);
        }

        /// <summary>
        /// Creates a single fire and forget webhook handler
        /// Need this here for now to select the handler for the callback
        /// </summary>
        /// <param name="webhookConfig">Webhook configuration</param>
        /// <returns></returns>
        public IHandler CreateWebhookHandler(WebhookConfig webhookConfig)
        {
            if (webhookConfig == null)
            {
                throw new Exception("Boom, handler webhook not found cannot process the message");
            }

            return new GenericWebhookHandler(
                _httpSender,
                _authenticationHandlerFactory,
                _requestBuilder,
                _requestLogger,
                _bigBrother,
                webhookConfig);
        }
    }
}
