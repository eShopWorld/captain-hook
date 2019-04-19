using System;
using System.Collections.Generic;
using Autofac.Features.Indexed;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    /// <summary>
    /// Selects the correct authentication handler based on the type specified by the authentication type.
    /// This implemented both Basic, OIDC and a custom implemented which will be moved to an integration layer.
    /// </summary>
    public class AuthenticationHandlerFactory : IAuthHandlerFactory
    {
        private Dictionary<string, IAcquireTokenHandler> handlers;
        private readonly IIndex<string, WebhookConfig> _webHookConfigs;
        private readonly IBigBrother _bigBrother;

        public AuthenticationHandlerFactory(IIndex<string, WebhookConfig> webHookConfigs, IBigBrother bigBrother)
        {
            _webHookConfigs = webHookConfigs;
            _bigBrother = bigBrother;

            handlers = new Dictionary<string, IAcquireTokenHandler>();
        }

        public IAcquireTokenHandler Get(string name)
        {
            if (!_webHookConfigs.TryGetValue(name.ToLower(), out var config))
            {
                throw new Exception($"Authentication Provider {name} not found");
            }

            if (handlers.ContainsKey(name))
            {
                return handlers[name];
            }

            switch (config.AuthenticationConfig.Type)
            {
                case AuthenticationType.None:
                    return null;

                case AuthenticationType.Basic:
                    handlers[name] = new BasicAuthenticationHandler(config.AuthenticationConfig);
                    break;

                case AuthenticationType.OIDC:
                    handlers[name] = new OidcAuthenticationHandler(config.AuthenticationConfig, _bigBrother);
                    break;

                case AuthenticationType.Custom:
                    //todo hack for now until we move this out of here and into an integration layer
                    //todo if this is custom it should be another webhook which calls out to another place, this place gets a token on CH's behalf and then adds this into subsequent webhook requests.
                    handlers[name] = new MmAuthenticationHandler(config.AuthenticationConfig, _bigBrother);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(config.AuthenticationConfig.Type),
                        $"unknown configuration type of {config.AuthenticationConfig.Type}");
            }

            return handlers[name];
        }
    }
}
