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
        private readonly IIndex<string, HttpClient> _httpClients;
        private readonly IBigBrother _bigBrother;
        private readonly IIndex<string, EventHandlerConfig> _eventHandlerConfig;
        private readonly IAuthHandlerFactory _authHandlerFactory;

        public EventHandlerFactory(
            IIndex<string, HttpClient> httpClients,
            IBigBrother bigBrother,
            IIndex<string, EventHandlerConfig> eventHandlerConfig,
            IAuthHandlerFactory authHandlerFactory)
        {
            _httpClients = httpClients;
            _bigBrother = bigBrother;
            _eventHandlerConfig = eventHandlerConfig;
            _authHandlerFactory = authHandlerFactory;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create the custom handler such that we get a mapping from the webhook to the handler selected
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public IHandler CreateHandler(string eventType)
        {
            if (!_eventHandlerConfig.TryGetValue(eventType.ToLower(), out var eventHandlerConfig))
            {
                throw new Exception("Boom, handler eventType not found cannot process the message");
            }
            
            var authHandler = _authHandlerFactory.Get($"{eventType}-webhook");
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
    }
}
