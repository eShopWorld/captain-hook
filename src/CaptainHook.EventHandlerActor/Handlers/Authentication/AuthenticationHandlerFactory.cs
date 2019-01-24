using System;
using Autofac.Features.Indexed;
using CaptainHook.Common;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public class AuthenticationHandlerFactory : IAuthHandlerFactory
    {
        private readonly IIndex<string, WebhookConfig> _webHookConfigs;
        private readonly IBigBrother _bigBrother;

        public AuthenticationHandlerFactory(IIndex<string, WebhookConfig> webHookConfigs, IBigBrother bigBrother)
        {
            _webHookConfigs = webHookConfigs;
            _bigBrother = bigBrother;
        }

        public IAuthenticationHandler Get(string name)
        {
            if (!_webHookConfigs.TryGetValue(name.ToLower(), out var config))
            {
                throw new Exception($"Authentication Provider {name} not found");
            }

            switch (name.ToLower())
            {
                case "max":
                case "dif":
                    return new MmOAuthAuthenticationHandler(config.AuthenticationConfig, _bigBrother);
                //todo hack for now remove in next pass
                case "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent-webhook":
                case "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent-callback":
                default:
                    return new OAuthAuthenticationHandler(config.AuthenticationConfig, _bigBrother);
            }
        }
    }
}
