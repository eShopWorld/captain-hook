using System;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class SubscriberConfigurationComparerTests
    {
        private readonly Dictionary<string, SubscriberConfiguration> oldConfigs = new Dictionary<string, SubscriberConfiguration>
        {
            ["event1-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("captain-hook").Create(),
            ["event1-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber1").Create(),
            ["event2-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook").Create(),
            ["event3-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event3").WithSubscriberName("captain-hook").Create(),
        };

        [Fact, IsLayer0]
        public void NoChangesDone_NoChangesDetected()
        {
            var newConfigs = new Dictionary<string, SubscriberConfiguration>
            {
                ["event1-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("captain-hook").Create(),
                ["event1-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber1").Create(),
                ["event2-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook").Create(),
                ["event3-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event3").WithSubscriberName("captain-hook").Create(),
            };

            var result = new SubscriberConfigurationComparer().Compare(oldConfigs, newConfigs);

            result.HasChanged.Should().BeFalse();
            result.Added.Should().BeEmpty();
            result.Removed.Should().BeEmpty();
            result.Changed.Should().BeEmpty();
        }

        [Fact, IsLayer0]
        public void NewSubscriber_ShouldBeInAddedList()
        {
            var newConfigs = new Dictionary<string, SubscriberConfiguration>
            {
                ["event1-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("captain-hook").Create(),
                ["event1-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber1").Create(),
                ["event2-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook").Create(),
                ["event2-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("subscriber1").Create(),
                ["event3-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event3").WithSubscriberName("captain-hook").Create(),
            };

            var result = new SubscriberConfigurationComparer().Compare(oldConfigs, newConfigs);

            result.HasChanged.Should().BeTrue();
            result.Added.Should().HaveCount(1).And.ContainKey("event2-subscriber1");
            result.Removed.Should().BeEmpty();
            result.Changed.Should().BeEmpty();
        }

        [Fact, IsLayer0]
        public void RemovedSubscriber_ShouldBeInRemovedList()
        {
            var newConfigs = new Dictionary<string, SubscriberConfiguration>
            {
                ["event1-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("captain-hook").Create(),
                ["event1-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber1").Create(),
                ["event3-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event3").WithSubscriberName("captain-hook").Create(),
            };

            var result = new SubscriberConfigurationComparer().Compare(oldConfigs, newConfigs);

            result.HasChanged.Should().BeTrue();
            result.Added.Should().BeEmpty();
            result.Removed.Should().HaveCount(1).And.ContainKey("event2-subscriber1");
            result.Changed.Should().BeEmpty();
        }

        [Theory, IsLayer0]
        [MemberData(nameof(ChangedSubscribers))]
        public void ExistingSubscriberChange_ShouldBeInChangedList(SubscriberConfiguration changedSubscriber)
        {
            var newConfigs = new Dictionary<string, SubscriberConfiguration>
            {
                ["event1-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("captain-hook").Create(),
                ["event1-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber1").Create(),
                ["event2-captain-hook"] = changedSubscriber,
                ["event3-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event3").WithSubscriberName("captain-hook").Create(),
            };

            var result = new SubscriberConfigurationComparer().Compare(oldConfigs, newConfigs);

            result.HasChanged.Should().BeTrue();
            result.Added.Should().BeEmpty();
            result.Removed.Should().BeEmpty();
            result.Changed.Should().HaveCount(1).And.ContainKey("event2-subscriber1");
        }

        public static IEnumerable<object[]> ChangedSubscribers
        {
            get
            {
                yield return new object[] { new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook").WithOidcAuthentication().Create() };
                yield return new object[] { new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook").WithCallback("https://calback.eshopworld.com").Create() };
                yield return new object[] { new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook")
                    .AddWebhookRequestRule(x => new WebhookRequestRuleBuilder().WithSource("OrderDto", DataType.Model).WithDestination("", DataType.Model).Create())
                    .Create() };
            }
        }
    }

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
        private List<WebhookRequestRule> webhookRequestRules = new List<WebhookRequestRule>();
        private WebhookConfig callback;

        public SubscriberConfigurationBuilder()
        {
            this.webhookRequestRules.Add(new WebhookRequestRuleBuilder().WithSource("RequestDto").WithDestination().Create());
        }

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

        public SubscriberConfigurationBuilder WithCallback(string uri)
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

    internal class WebhookRequestRuleBuilder
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
                Source = this.source,
                Destination = this.destination,
            };

            return rule;
        }
    }
}
