using System;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Tests.Builders
{
    internal class SubscriberConfigurationBuilder
    {
        private string type = "event1";
        private string subscriberName = "captain-hook";
        private string uri = "https://blah.blah.eshopworld.com";
        private HttpMethod httpMethod = HttpMethod.Post;
        private AuthenticationConfig authenticationConfig = new BasicAuthenticationConfig
        {
            Type = AuthenticationType.Basic,
            Username = "user",
            Password = "password",
        };
        private List<WebhookRequestRule> webhookRequestRules;
        private WebhookConfig callback;

        public SubscriberConfigurationBuilder WithType(string type)
        {
            this.type = type;
            return this;
        }

        public SubscriberConfigurationBuilder WithSubscriberName(string subscriberName)
        {
            this.subscriberName = subscriberName;
            return this;
        }

        public SubscriberConfigurationBuilder WithUri(string uri)
        {
            this.uri = uri;
            return this;
        }

        public SubscriberConfigurationBuilder WithOidcAuthentication()
        {
            this.authenticationConfig = new OidcAuthenticationConfig
            {
                Type = AuthenticationType.OIDC,
                Uri = "https://blah-blah.sts.eshopworld.com",
                ClientId = "ClientId",
                ClientSecret = "ClientSecret",
                Scopes = new[] { "scope1", "scope2" }
            };

            return this;
        }

        public SubscriberConfigurationBuilder WithCallback(string uri = "https://calback.eshopworld.com")
        {
            this.callback = new WebhookConfig
            {
                Name = "callback",
                HttpMethod = HttpMethod.Post,
                Uri = uri,
                EventType = "event1",
                AuthenticationConfig = new AuthenticationConfig
                {
                    Type = AuthenticationType.None
                },
            };

            return this;
        }

        public SubscriberConfigurationBuilder AddWebhookRequestRule(Action<WebhookRequestRuleBuilder> ruleBuilder)
        {
            if (this.webhookRequestRules == null)
            {
                this.webhookRequestRules = new List<WebhookRequestRule>();
            }

            var rb = new WebhookRequestRuleBuilder();
            ruleBuilder(rb);
            this.webhookRequestRules.Add(rb.Create());
            return this;
        }

        public SubscriberConfiguration Create()
        {
            var subscriber = new SubscriberConfiguration
            {
                Name = this.type,
                EventType = this.type,
                SubscriberName = this.subscriberName,
                HttpMethod = this.httpMethod,
                Uri = this.uri,
                AuthenticationConfig = this.authenticationConfig,
                Callback = this.callback,
                WebhookRequestRules = this.webhookRequestRules,
            };

            return subscriber;
        }
    }
}