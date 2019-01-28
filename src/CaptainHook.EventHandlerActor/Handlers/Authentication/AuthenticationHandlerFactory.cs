using System;
using Autofac.Features.Indexed;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    /// <summary>
    /// Selects the correct authentication handler based on the type specified by the authentication type.
    /// This implemented both Basic, OAuth and a custom implemented which will be moved to an integration layer.
    /// </summary>
    public class AuthenticationHandlerFactory : IAuthHandlerFactory
    {
        private readonly IIndex<string, WebhookConfig> _webHookConfigs;
        private readonly IBigBrother _bigBrother;

        public AuthenticationHandlerFactory(IIndex<string, WebhookConfig> webHookConfigs, IBigBrother bigBrother)
        {
            _webHookConfigs = webHookConfigs;
            _bigBrother = bigBrother;
        }

        public IAcquireTokenHandler Get(string name)
        {
            if (!_webHookConfigs.TryGetValue(name.ToLower(), out var config))
            {
                throw new Exception($"Authentication Provider {name} not found");
            }

            switch (config.AuthenticationType)
            {
                case AuthenticationType.None:
                    return null;
                case AuthenticationType.Basic:
                    return new BasicAcquireTokenHandler(config.AuthenticationConfig);
                case AuthenticationType.OAuth:
                    return new OAuthTokenHandler(config.AuthenticationConfig);
                case AuthenticationType.Custom:
                    //todo hack for now until we move this out of here and into an integration layer
                    //todo if this is custom it should be another webhook which calls out to another place, this place gets a token on CH's behalf and then adds this into subsequent webhook requests.
                    return new MmOAuthAuthenticationHandler(config.AuthenticationConfig);
                default:
                    throw new ArgumentOutOfRangeException(nameof(config.AuthenticationType), $"unknown configuration type of {config.AuthenticationType}");
            }
        }
    }
}
