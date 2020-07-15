using System;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Tests.Builders
{
    internal class WebhookConfigBuilder
    {
        private string _type = "event1";
        private string _uri = "https://blah.blah.eshopworld.com";
        private HttpMethod _httpMethod = HttpMethod.Post;
        private AuthenticationConfig _authenticationConfig = new BasicAuthenticationConfig
        {
            Type = AuthenticationType.Basic,
            Username = "user",
            Password = "password",
        };
        private List<WebhookRequestRule> _webhookRequestRules;

        public WebhookConfigBuilder WithType(string type)
        {
            _type = type;
            return this;
        }

        public WebhookConfigBuilder WithUri(string uri)
        {
            _uri = uri;
            return this;
        }

        public WebhookConfigBuilder WithOidcAuthentication()
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

        public WebhookConfigBuilder WithBasicAuthentication()
        {
            _authenticationConfig = new BasicAuthenticationConfig
            {
                Type = AuthenticationType.Basic,
                Username = "username",
                Password = "password",
            };

            return this;
        }

        public WebhookConfigBuilder SetWebhookRequestRule(Action<WebhookRequestRuleBuilder> ruleBuilder)
        {            
            var builder = new WebhookRequestRuleBuilder();
            ruleBuilder(builder);
            _webhookRequestRules = new List<WebhookRequestRule>
            {
                builder.Create()
            };
            return this;
        }

        public WebhookConfigBuilder AddWebhookRequestRule(Action<WebhookRequestRuleBuilder> ruleBuilder)
        {
            if (_webhookRequestRules == null)
            {
                _webhookRequestRules = new List<WebhookRequestRule>();
            }

            var builder = new WebhookRequestRuleBuilder();
            ruleBuilder(builder);
            _webhookRequestRules.Add(builder.Create());
            return this;
        }

        public WebhookConfig Create()
        {
            var webhookConfig = new WebhookConfig
            {
                Name = _type,
                EventType = _type,
                HttpMethod = _httpMethod,
                Uri = _uri,
                AuthenticationConfig = _authenticationConfig,
                WebhookRequestRules = _webhookRequestRules,
            };

            return webhookConfig;
        }
    }
}