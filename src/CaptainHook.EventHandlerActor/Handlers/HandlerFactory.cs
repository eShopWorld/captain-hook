using System;
using System.Net.Http;
using Autofac.Features.Indexed;
using CaptainHook.Common;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    //todo remove in v1
    public class HandlerFactory : IHandlerFactory
    {
        private readonly IIndex<string, HttpClient> _httpClients;
        private readonly IBigBrother _bigBrother;
        private readonly IIndex<string, EventHandlerConfig> _webHookConfig;
        private readonly IAuthHandlerFactory _authHandlerFactory;

        public HandlerFactory(
            IIndex<string, HttpClient> httpClients,
            IBigBrother bigBrother,
            IIndex<string, EventHandlerConfig> webHookConfig,
            IAuthHandlerFactory authHandlerFactory)
        {
            _httpClients = httpClients;
            _bigBrother = bigBrother;
            _webHookConfig = webHookConfig;
            _authHandlerFactory = authHandlerFactory;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create the custom handler such that we get a mapping from the webhook name to the handler selected
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHandler CreateHandler(string name)
        {
            if (!_webHookConfig.TryGetValue(name.ToLower(), out var webhookConfig))
            {
                throw new Exception("Boom, don't know the brand type");
            }

            var authHandler = _authHandlerFactory.Get(name);

            switch (name.ToLower())
            {
                //todo not needed in v1
                case "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent":
                    return new WebhookResponseHandler(
                        this,
                        authHandler,
                        _bigBrother,
                        _httpClients[name.ToLower()], 
                        webhookConfig);
                
                case "checkout.domain.infrastructure.domainevents.platformordercreatedomainevent":
                case "esw":
                    return new GenericWebhookHandler(
                        authHandler, 
                        _bigBrother, 
                        _httpClients[name.ToLower()], 
                        webhookConfig.WebHookConfig);
                
                default:
                    throw new Exception($"Boom, don't know the domain type or handler name {name}");
            }
        }
    }
}
