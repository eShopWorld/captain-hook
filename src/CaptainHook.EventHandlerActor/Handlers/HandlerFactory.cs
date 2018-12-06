namespace CaptainHook.EventHandlerActor.Handlers
{
    using System;
    using System.Net.Http;
    using Authentication;
    using Autofac.Features.Indexed;
    using Common;
    using Eshopworld.Core;

    public class HandlerFactory : IHandlerFactory
    {
        private readonly IIndex<string, HttpClient> _httpClients;
        private readonly IBigBrother _bigBrother;
        private readonly IIndex<string, WebHookConfig> _webHookConfig;
        private readonly IAuthHandlerFactory _authHandlerFactory;

        public HandlerFactory(
            IIndex<string, HttpClient> httpClients,
            IBigBrother bigBrother,
            IIndex<string, WebHookConfig> webHookConfig,
            IAuthHandlerFactory authHandlerFactory)
        {
            _httpClients = httpClients;
            _bigBrother = bigBrother;
            _webHookConfig = webHookConfig;
            _authHandlerFactory = authHandlerFactory;
        }

        /// <summary>
        /// Create the custom handler such that we get a mapping from the brandtype to the registered handler
        /// </summary>
        /// <param name="brandType"></param>
        /// <param name="domainType"></param>
        /// <returns></returns>
        public IHandler CreateHandler(string brandType, string domainType)
        {
            if (!_webHookConfig.TryGetValue(brandType.ToUpper(), out var webhookConfig))
            {
                throw new Exception("Boom, don't know the brand type");
            }

            var tokenHandler = _authHandlerFactory.Get(brandType);

            switch ($"{brandType.ToLower()}-{domainType.ToLower()}")
            {
                case "max-checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent":
                case "dif-checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent":
                    return new MmEventHandler(this, _httpClients[brandType], _bigBrother, webhookConfig, tokenHandler);

                case "max-checkout.domain.infrastructure.domainevents.platformordercreatedomainevent":
                case "dif-checkout.domain.infrastructure.domainevents.platformordercreatedomainevent":
                case "esw":
                    return new GenericEventHandler(tokenHandler, _bigBrother, _httpClients[brandType], webhookConfig);
                default:
                    throw new Exception("Boom, don't know the brand type");
            }
        }
    }
}