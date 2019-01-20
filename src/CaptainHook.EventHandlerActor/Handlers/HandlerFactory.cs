using System;
using System.Net.Http;
using Autofac.Features.Indexed;
using CaptainHook.Common;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class HandlerFactory : IHandlerFactory
    {
        private readonly IIndex<string, HttpClient> _httpClients;
        private readonly IBigBrother _bigBrother;
        private readonly IIndex<string, EventHandlerConfig> _eventHandlerConfig;
        private readonly IAuthHandlerFactory _authHandlerFactory;

        public HandlerFactory(
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

        /// <summary>
        /// Create the custom handler such that we get a mapping from the webhook name to the handler selected
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHandler CreateHandler(string name)
        {
            if (!_eventHandlerConfig.TryGetValue(name.ToLower(), out var eventHandlerConfig))
            {
                throw new Exception("Boom, don't know the brand type");
            }

            var authHandler = _authHandlerFactory.Get(name);

            if (eventHandlerConfig.CallBackEnabled)
            {
                return new WebhookResponseHandler(
                    this,
                    authHandler,
                    _bigBrother,
                    _httpClients[name.ToLower()],
                    eventHandlerConfig);
            }

            return new GenericWebhookHandler(
                authHandler,
                _bigBrother,
                _httpClients[name.ToLower()],
                eventHandlerConfig.WebHookConfig);
        }
    }
}
