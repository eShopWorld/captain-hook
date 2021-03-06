﻿using System;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class WebhookConfigRouteBuilder
    {
        private string _selector;
        private string _uri;
        private string _httpVerb = "POST";
        private TimeSpan[] _retrySleepDurations;
        private TimeSpan? _httpTimeout;
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

        public WebhookConfigRouteBuilder WithHttpTimeout(TimeSpan httpTimeout)
        {
            _httpTimeout = httpTimeout;
            return this;
        }

        public WebhookConfigRouteBuilder WithRetrySleepDurations(TimeSpan[] retrySleepDurations)
        {
            _retrySleepDurations = retrySleepDurations;
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

        public WebhookConfigRouteBuilder WithNoAuthentication()
        {
            _authenticationConfig = new AuthenticationConfig
            {
                Type = AuthenticationType.None
            };

            return this;
        }

        public WebhookConfigRouteBuilder WithAuthentication(AuthenticationConfig authenticationConfig)
        {
            _authenticationConfig = authenticationConfig;
            return this;
        }

        public WebhookConfigRouteBuilder WithHttpVerb(string verb)
        {
            _httpVerb = verb;
            return this;
        }

        public WebhookConfigRoute Create()
        {
            var route = new WebhookConfigRoute
            {
                Selector = _selector,
                Uri = _uri,
                AuthenticationConfig = _authenticationConfig,
                HttpVerb = _httpVerb,
            };

            if (_retrySleepDurations != null)
            {
                route.RetrySleepDurations = _retrySleepDurations;
            }

            if (_httpTimeout.HasValue)
            {
                route.Timeout = _httpTimeout.Value;
            }

            return route;
        }
    }
}