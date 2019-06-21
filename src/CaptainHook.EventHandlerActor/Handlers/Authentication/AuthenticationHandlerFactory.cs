using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly ConcurrentDictionary<string, IAcquireTokenHandler> _handlers;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IIndex<string, WebhookConfig> _webHookConfigs;
        private readonly IBigBrother _bigBrother;

        public AuthenticationHandlerFactory(IIndex<string, WebhookConfig> webHookConfigs, IBigBrother bigBrother)
        {
            _webHookConfigs = webHookConfigs;
            _bigBrother = bigBrother;

            _handlers = new ConcurrentDictionary<string, IAcquireTokenHandler>();
        }

        /// <summary>
        /// Get the token provider based on host
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IAcquireTokenHandler> GetAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var host = uri.Host;
            return await GetAsync(host, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IAcquireTokenHandler> GetAsync(string name, CancellationToken cancellationToken)
        {
            if (!_webHookConfigs.TryGetValue(name.ToLower(), out var config))
            {
                throw new Exception($"Authentication Provider {name} not found");
            }

            if (_handlers.TryGetValue(name, out var handler))
            {
                return handler;
            }
            
            switch (config.AuthenticationConfig.Type)
            {
                case AuthenticationType.None:
                    return null;

                case AuthenticationType.Basic:
                    await EnterSemaphore(() =>
                    {
                        _handlers.TryAdd(name, new BasicAuthenticationHandler(config.AuthenticationConfig));
                    }, name, cancellationToken);
                    break;

                case AuthenticationType.OIDC:
                    await EnterSemaphore(() =>
                    {
                        _handlers.TryAdd(name, new OidcAuthenticationHandler(config.AuthenticationConfig, _bigBrother));
                    }, name, cancellationToken);
                    break;

                case AuthenticationType.Custom:

                    //todo hack for now until we move this out of here and into an integration layer
                    //todo if this is custom it should be another webhook which calls out to another place, this place gets a token on CH's behalf and then adds this into subsequent webhook requests.
                    await EnterSemaphore(() =>
                    {
                        _handlers.TryAdd(name, new MmAuthenticationHandler(config.AuthenticationConfig, _bigBrother));
                    }, name, cancellationToken);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(config.AuthenticationConfig.Type),
                        $"unknown configuration type of {config.AuthenticationConfig.Type}");
            }

            _handlers.TryGetValue(name, out var newHandler);
            return newHandler;
        }

        private async Task EnterSemaphore(Action action, string key, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                if (_handlers.ContainsKey(key))
                {
                    return;
                }
                action();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
