using System;
using System.Net.Http;
using Autofac.Features.Indexed;
using CaptainHook.Common;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class EventEventHandlerFactory : IEventHandlerFactory
    {
        private readonly IIndex<string, HttpClient> _httpClients;
        private readonly IBigBrother _bigBrother;
        private readonly IIndex<string, EventHandlerConfig> _eventHandlerConfig;
        private readonly IIndex<string, WebhookConfig> _webHookConfig;
        private readonly IAuthHandlerFactory _authHandlerFactory;

        public EventEventHandlerFactory(
            IIndex<string, HttpClient> httpClients,
            IBigBrother bigBrother,
            IIndex<string, EventHandlerConfig> eventHandlerConfig,
            IIndex<string, WebhookConfig> webHookConfig,
            IAuthHandlerFactory authHandlerFactory)
        {
            _httpClients = httpClients;
            _bigBrother = bigBrother;
            _eventHandlerConfig = eventHandlerConfig;
            _webHookConfig = webHookConfig;
            _authHandlerFactory = authHandlerFactory;
        }

        /// <summary>
        /// Create the custom handler such that we get a mapping from the webhook to the handler selected
        /// </summary>
        /// <param name="fullEventName"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public IHandler CreateHandler(string fullEventName, string eventType)
        {
            if (!_eventHandlerConfig.TryGetValue(fullEventName.ToLower(), out var eventHandlerConfig))
            {
                throw new Exception("Boom, handler eventType not found cannot process the message");
            }
            
            var authHandler = _authHandlerFactory.Get(eventType);

            if (eventHandlerConfig.CallBackEnabled)
            {
                return new WebhookResponseHandler(
                    this,
                    authHandler,
                    _bigBrother,
                    _httpClients[eventHandlerConfig.WebHookConfig.Name.ToLower()],
                    eventHandlerConfig);
            }

            return new GenericWebhookHandler(
                authHandler,
                _bigBrother,
                _httpClients[eventHandlerConfig.WebHookConfig.Name.ToLower()],
                eventHandlerConfig.WebHookConfig);
        }

        /// <summary>
        /// Creates a single fire and forget webhook handler
        /// </summary>
        /// <param name="webHookName"></param>
        /// <returns></returns>
        public IHandler CreateHandler(string webHookName)
        {
            if (!_webHookConfig.TryGetValue(webHookName.ToLower(), out var webhookConfig))
            {
                throw new Exception("Boom, handler webhook not found cannot process the message");
            }

            var authHandler = _authHandlerFactory.Get(webHookName);
            
            return new GenericWebhookHandler(
                authHandler,
                _bigBrother,
                _httpClients[webHookName.ToLower()],
                webhookConfig);
        }
    }
}
