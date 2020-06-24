﻿using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Tests.Builders
{
    internal class WebhookConfigRouteBuilder
    {
        private string _selector;
        private string _uri;
        private AuthenticationConfig _authenticationConfig = new BasicAuthenticationConfig();

        public WebhookConfigRouteBuilder WithSelector(string selector)
        {
            _selector = selector;
            return this;
        }

        public WebhookConfigRouteBuilder WithUri(string uri)
        {
            _uri = uri;
            return this;
        }

        public WebhookConfigRouteBuilder WithOidcAuthentication()
        {
            _authenticationConfig = new OidcAuthenticationConfig
            {
                Type = AuthenticationType.OIDC,
                Uri = "https://blah-blah.sts.eshopworld.com",
                ClientId = "ClientId",
                ClientSecret = "ClientSecret",
                Scopes = new[] { "scope1", "scope2" }
            };

            return this;
        }

        public WebhookConfigRouteBuilder WithBasicAuthentication()
        {
            _authenticationConfig = new BasicAuthenticationConfig
            {
                Type = AuthenticationType.Basic,
                Username = "username",
                Password = "password",
            };

            return this;
        }

        public WebhookConfigRoute Create()
        {
            var route = new WebhookConfigRoute
            {
                Selector = _selector,
                Uri = _uri,
                AuthenticationConfig = _authenticationConfig,
                HttpVerb = "POST"
            };

            return route;
        }
    }
}