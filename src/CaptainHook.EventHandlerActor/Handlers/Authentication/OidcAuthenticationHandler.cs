﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Telemetry.Web;
using Eshopworld.Core;
using IdentityModel.Client;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    /// <summary>
    /// OAuth2 authentication handler.
    /// Gets a token from the supplied STS details included the supplied scopes.
    /// Requests token once
    /// </summary>
    public class OidcAuthenticationHandler : AuthenticationHandler, IAuthenticationHandler
    {
        //todo cache and make it thread safe, ideally should have one per each auth domain and have the expiry set correctly
        protected OidcAuthenticationToken OidcAuthenticationToken = new OidcAuthenticationToken();
        protected readonly OidcAuthenticationConfig OidcAuthenticationConfig;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        protected readonly IHttpSender HttpSender
            ;
        protected readonly IBigBrother BigBrother;

        public OidcAuthenticationHandler(IHttpSender httpSender, AuthenticationConfig authenticationConfig, IBigBrother bigBrother)
        {
            var oAuthAuthenticationToken = authenticationConfig as OidcAuthenticationConfig;
            OidcAuthenticationConfig = oAuthAuthenticationToken ?? throw new ArgumentException($"configuration for basic authentication is not of type {typeof(OidcAuthenticationConfig)}", nameof(authenticationConfig));
            HttpSender = httpSender;
            BigBrother = bigBrother;
        }

        /// <inheritdoc />
        public virtual async Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            //get initial access token and refresh token
            if (OidcAuthenticationToken.AccessToken == null)
            {
                BigBrother.Publish(new ClientTokenRequest
                {
                    ClientId = OidcAuthenticationConfig.ClientId,
                    Authority = OidcAuthenticationConfig.Uri,
                    Message = "Token is null getting a new one."
                });

                await EnterSemaphore(cancellationToken, async () =>
                {
                    var response = await GetTokenResponseAsync(cancellationToken);
                    ReportTokenUpdateFailure(OidcAuthenticationConfig, response);
                    UpdateToken(response);

                    BigBrother.Publish(new ClientTokenRequest
                    {
                        ClientId = OidcAuthenticationConfig.ClientId,
                        Authority = OidcAuthenticationConfig.Uri,
                        Message = "Got a new token and updating the http client when token was null"
                    });
                });
            }
            else if (CheckExpired())
            {
                await EnterSemaphore(cancellationToken, async () =>
                {
                    if (CheckExpired())
                    {
                        var response = await GetTokenResponseAsync(cancellationToken);
                        ReportTokenUpdateFailure(OidcAuthenticationConfig, response);
                        UpdateToken(response);

                        BigBrother.Publish(new ClientTokenRequest
                        {
                            ClientId = OidcAuthenticationConfig.ClientId,
                            Authority = OidcAuthenticationConfig.Uri,
                            Message = "Refreshing token and updating the http client with new token"
                        });
                    }
                });
            }

            return $"Bearer {OidcAuthenticationToken.AccessToken}";
        }

        public bool HasConfigChanged(AuthenticationConfig newConfig)
        {
            if (newConfig is OidcAuthenticationConfig oidcNewConfig)
            {
                var equals = oidcNewConfig.Uri == OidcAuthenticationConfig.Uri
                               && oidcNewConfig.ClientId == OidcAuthenticationConfig.ClientId
                               && oidcNewConfig.ClientSecret == OidcAuthenticationConfig.ClientSecret
                               && oidcNewConfig.GrantType == OidcAuthenticationConfig.GrantType
                               && oidcNewConfig.RefreshBeforeInSeconds == OidcAuthenticationConfig.RefreshBeforeInSeconds;
                
                if (oidcNewConfig.Scopes != null)
                {
                    if (OidcAuthenticationConfig.Scopes != null)
                        equals = equals && oidcNewConfig.Scopes.SequenceEqual(OidcAuthenticationConfig.Scopes);
                    else
                        equals = false;
                }

                return !equals;
            }

            return true;
        }

        /// <summary>
        /// Makes the call to get the token
        /// </summary>
        /// <param name="client"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<TokenResponse> GetTokenResponseAsync(CancellationToken token)
        {
            var response = await HttpSender.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = OidcAuthenticationConfig.Uri,
                ClientId = OidcAuthenticationConfig.ClientId,
                ClientSecret = OidcAuthenticationConfig.ClientSecret,
                GrantType = OidcAuthenticationConfig.GrantType,
                Scope = string.Join(" ", OidcAuthenticationConfig.Scopes)
            }, token);

            return response;
        }

        /// <summary>
        /// Updates the local cached token
        /// </summary>
        /// <param name="response"></param>
        protected void UpdateToken(TokenResponse response)
        {
            OidcAuthenticationToken.AccessToken = response.AccessToken;
            OidcAuthenticationToken.RefreshToken = response.RefreshToken;
            OidcAuthenticationToken.ExpiresIn = response.ExpiresIn;
            OidcAuthenticationToken.TimeOfRefresh = ServerDateTime.UtcNow;
        }

        /// <summary>
        /// Quackulation to determine when to renew the token
        /// </summary>
        /// <returns></returns>
        private bool CheckExpired()
        {
            return ServerDateTime.UtcNow.Subtract(OidcAuthenticationToken.TimeOfRefresh).TotalSeconds >= OidcAuthenticationToken.ExpiresIn - OidcAuthenticationConfig.RefreshBeforeInSeconds;
        }

        private async Task EnterSemaphore(CancellationToken cancellationToken, Func<Task> action)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                await action();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
