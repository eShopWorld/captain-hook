namespace CaptainHook.EventHandlerActor.Handlers
{
    using System.Net.Http;
    using Autofac.Features.Indexed;
    using Common.Authentication;
    using Eshopworld.Core;

    public class HandlerFactory : IHandlerFactory
    {
        private readonly HttpClient _client;
        private readonly IBigBrother _bigBrother;
        private readonly IIndex<string, WebHookConfig> _webHookConfig;
        private readonly IAuthHandlerFactory _authHandlerFactory;

        public HandlerFactory(
            HttpClient client,
            IBigBrother bigBrother,
            IIndex<string, WebHookConfig> webHookConfig,
            IAuthHandlerFactory authHandlerFactory)
        {
            _client = client;
            _bigBrother = bigBrother;
            _webHookConfig = webHookConfig;
            _authHandlerFactory = authHandlerFactory;
        }

        /// <summary>
        /// Create the custom handler such that we get a mapping from the brandtype to the registered handler
        /// </summary>
        /// <param name="brandType"></param>
        /// <returns></returns>
        public IHandler CreateHandler(string brandType)
        {
            if (_webHookConfig.TryGetValue(brandType.ToUpper(), out var webhookConfig))
            {
                var tokenHandler = _authHandlerFactory.Get(brandType);

                switch (brandType.ToUpper())
                {
                    case "MAX":
                    case "DIF":
                        return new MmEventHandler(this, _client, _bigBrother, webhookConfig, tokenHandler);
                    default:
                        return new GenericEventHandler(tokenHandler, _bigBrother, _client, webhookConfig);
                }
            }
            else
            {
                //todo handle unknown webhook
                return null;
            }
        }
    }
}