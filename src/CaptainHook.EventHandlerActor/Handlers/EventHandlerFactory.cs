using System;
using Autofac.Features.Indexed;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class EventHandlerFactory : IEventHandlerFactory
    {
        private readonly IBigBrother _bigBrother;
        private readonly IIndex<string, SubscriberConfiguration> _subscriberConfigurations;
        private readonly IIndex<string, WebhookConfig> _webHookConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthenticationHandlerFactory _authenticationHandlerFactory;
        private readonly IRequestLogger _requestLogger;
        private readonly IRequestBuilder _requestBuilder;

        public EventHandlerFactory(
            IBigBrother bigBrother,
            IIndex<string, SubscriberConfiguration> subscriberConfigurations,
            IIndex<string, WebhookConfig> webHookConfig,
            IHttpClientFactory httpClientFactory,
            IAuthenticationHandlerFactory authenticationHandlerFactory,
            IRequestLogger requestLogger,
            IRequestBuilder requestBuilder)
        {
            _bigBrother = bigBrother;
            _subscriberConfigurations = subscriberConfigurations;
            _httpClientFactory = httpClientFactory;
            _requestLogger = requestLogger;
            _requestBuilder = requestBuilder;
            _authenticationHandlerFactory = authenticationHandlerFactory;
            _webHookConfig = webHookConfig;
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
                // for backward compatibility, if "old" version of reader send MessageData without SubscriberConfig, try to load it from configuration as it was previously
                if (!_subscriberConfigurations.TryGetValue(SubscriberConfiguration.Key(messageData.Type, messageData.SubscriberName), out subscriberConfig))
                {
                    throw new Exception($"Boom, handler event type '{messageData.Type}' was not found, cannot process the message");
                }

                messageData.SubscriberConfig = subscriberConfig;
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
                // for backward compatibility, if "old" version of reader send MessageData without WebhookConfig, try to load it from configuration as it was previously
                if (!_webHookConfig.TryGetValue(webHookName.ToLowerInvariant(), out webhookConfig))
                {
                    throw new Exception("Boom, handler webhook not found cannot process the message");
                }
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
