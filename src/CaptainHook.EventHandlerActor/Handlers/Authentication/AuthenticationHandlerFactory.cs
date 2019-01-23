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

        public IAuthHandler Get(string name)
        {
            if (!_webHookConfigs.TryGetValue(name.ToLower(), out var config))
            {
                throw new Exception($"Authentication Provider {name} not found");
            }

            switch (name.ToLower())
            {
                case "max":
                case "dif":
                    return new MmAuthenticationHandler(config.AuthenticationConfig, _bigBrother);
                default:
                    return new AuthenticationHandler(config.AuthenticationConfig, _bigBrother);
            }
        }
    }
}
