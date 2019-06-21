using System;
using System.Net.Http;
using Autofac.Features.Indexed;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class EventHandlerFactory : IEventHandlerFactory
    {
        private readonly IBigBrother _bigBrother;
        private readonly IIndex<string, EventHandlerConfig> _eventHandlerConfig;
        private readonly IIndex<string, WebhookConfig> _webHookConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthenticationHandlerFactory _authenticationHandlerFactory;

        public EventHandlerFactory(
            IBigBrother bigBrother,
            IIndex<string, EventHandlerConfig> eventHandlerConfig,
            IIndex<string, WebhookConfig> webHookConfig,
            IHttpClientFactory httpClientFactory,
            IAuthenticationHandlerFactory authenticationHandlerFactory)
        {
            _bigBrother = bigBrother;
            _eventHandlerConfig = eventHandlerConfig;
            _authenticationHandlerFactory = authenticationHandlerFactory;
            _webHookConfig = webHookConfig;
            _httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create the custom handler such that we get a mapping from the webhook to the handler selected
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public IHandler CreateEventHandler(string eventType)
        {
            if (!_eventHandlerConfig.TryGetValue(eventType.ToLower(), out var eventHandlerConfig))
            {
                throw new Exception($"Boom, handler event type {eventType} was not found, cannot process the message");
            }

            if (eventHandlerConfig.CallBackEnabled)
            {
                return new WebhookResponseHandler(
                    this,
                    _authenticationHandlerFactory,
                    new RequestBuilder(),
                    _bigBrother,
                    _httpClientFactory,
                    eventHandlerConfig);
            }

            return new GenericWebhookHandler(
                _authenticationHandlerFactory,
                new RequestBuilder(),
                _bigBrother,
                _httpClientFactory,
                eventHandlerConfig.WebhookConfig);
        }

        /// <summary>
        /// Creates a single fire and forget webhook handler
        /// Need this here for now to select the handler for the callback
        /// </summary>
        /// <param name="webHookName"></param>
        /// <returns></returns>
        public IHandler CreateWebhookHandler(string webHookName)
        {
            if (!_webHookConfig.TryGetValue(webHookName.ToLower(), out var webhookConfig))
            {
                throw new Exception("Boom, handler webhook not found cannot process the message");
            }

            return new GenericWebhookHandler(
                _authenticationHandlerFactory,
                new RequestBuilder(),
                _bigBrother,
                _httpClientFactory,
                webhookConfig);
        }
    }
}
