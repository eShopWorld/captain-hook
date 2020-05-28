using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService;
using Eshopworld.Tests.Core;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class SubscriberConfigurationComparerTests
    {
        [Fact, IsLayer0]
        public void ShouldDetectNewSubscriberConfiguration()
        {
            var oldConfigs = new Dictionary<string, SubscriberConfiguration>
            {
                ["event1-captain-hook"] = new SubscriberConfiguration(),
                ["event1-subscriber1"] = new SubscriberConfiguration(),
                ["event2-captain-hook"] = new SubscriberConfiguration(),
                ["event3-captain-hook"] = new SubscriberConfiguration(),
            };

            var newConfigs = new Dictionary<string, SubscriberConfiguration>
            {
                ["event1-captain-hook"] = new SubscriberConfiguration(),
                ["event1-subscriber1"] = new SubscriberConfiguration(),
                ["event2-captain-hook"] = new SubscriberConfiguration(),
                ["event3-captain-hook"] = new SubscriberConfiguration(),
            };

            var comparer = new SubscriberConfigurationComparer();
            var result = comparer.Compare(oldConfigs, newConfigs);
        }
    }

    internal class SubscriberConfigurationBuilder
    {
        private string name = "event1name";
        private string type = "event1type";
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

        public SubscriberConfigurationBuilder WithName(string name)
        {
            this.name = name;
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

        public SubscriberConfigurationBuilder WithCallback(string uri, HttpMethod httpMethod)
        {
            this.callback = new WebhookConfig
            {
                Name = "callback",
                HttpMethod = httpMethod,
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
                Name = this.name,
                EventType = this.type,
                HttpMethod = this.httpMethod,
                Uri = this.uri,
                AuthenticationConfig = this.authenticationConfig,
                Callback = this.callback,
                WebhookRequestRules = this.webhookRequestRules,
            };

            return subscriber;
        }
    }

    public class WebhookRequestRuleBuilder
    {
        private ParserLocation source;
        private ParserLocation destination;

        public WebhookRequestRuleBuilder WithSource(string path = null, DataType type = DataType.Property, Location location = Location.Body)
        {
            this.source = new ParserLocation
            {
                Path = path,
                Location = location,
                Type = type,
            };

            return this;
        }

        public WebhookRequestRuleBuilder WithDestination(string path = null, DataType type = DataType.Property, Location location = Location.Body)
        {
            this.destination = new ParserLocation
            {
                Path = path,
                Location = location,
                Type = type,
            };

            return this;
        }

        public WebhookRequestRule Create()
        {
            var rule = new WebhookRequestRule
            {
                Source = source,
                Destination = destination,
            };

            return rule;
        }
    }
}
