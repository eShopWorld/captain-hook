using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Domain.Entities;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Infrastructure
{
    public class SubscriberEntityToConfigurationMapperTests
    {
        private readonly Mock<ISecretProvider> _secretProviderMock = new Mock<ISecretProvider>();

        public SubscriberEntityToConfigurationMapperTests()
        {
            _secretProviderMock.Setup(m => m.GetSecretValueAsync("kv-secret-name")).ReturnsAsync("my-password");
        }

        [Theory, IsUnit]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("PUT")]
        public async Task MapSubscriberAsync_WithSingleWebhookAndNoUriTransformDefined_MapsToSingleWebhook(string httpVerb)
        {
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
                "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, string.Empty, authentication: authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.Should().HaveCount(1);
                var subscriberConfiguration = result.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().Be("https://blah-blah.eshopworld.com/webhook/");
                subscriberConfiguration.HttpMethod.Should().Be(new HttpMethod(httpVerb));
                subscriberConfiguration.AuthenticationConfig.Should().BeOfType<OidcAuthenticationConfig>();
                var authenticationConfig = subscriberConfiguration.AuthenticationConfig as OidcAuthenticationConfig;
                authenticationConfig.Uri.Should().Be("https://blah-blah.sts.eshopworld.com");
                authenticationConfig.Scopes.Should().Contain(new[] { "scope1" });
                authenticationConfig.Type.Should().Be(AuthenticationType.OIDC);
                authenticationConfig.ClientId.Should().Be("captain-hook-id");
            }
        }

        [Theory, IsUnit]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("PUT")]
        public async Task MapSubscriberAsync_WithSingleWebhookAndNoSelectionRuleAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb)
        {
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
               "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhookUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.Should().HaveCount(1);
                var subscriberConfiguration = result.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().BeNull();
                subscriberConfiguration.WebhookRequestRules.Count.Should().Be(1);
                var rule = subscriberConfiguration.WebhookRequestRules.Single();
                rule.Routes.Count.Should().Be(1);
                rule.Routes[0].Uri.Should().Be("https://blah-{orderCode}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("*");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("orderCode").WhichValue.Should().Be("$.OrderCode");

                rule.Routes[0].AuthenticationConfig.Should().BeOfType<OidcAuthenticationConfig>();
                var authenticationConfig = rule.Routes[0].AuthenticationConfig as OidcAuthenticationConfig;
                authenticationConfig.Uri.Should().Be("https://blah-blah.sts.eshopworld.com");
                authenticationConfig.Scopes.Should().Contain(new[] { "scope1" });
                authenticationConfig.Type.Should().Be(AuthenticationType.OIDC);
                authenticationConfig.ClientId.Should().Be("captain-hook-id");
            }
        }

        [Theory, IsUnit]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("PUT")]
        public async Task MapSubscriberAsync_WithMultipleWebhooksAndDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb)
        {
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
               "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhookSelectionRule("$.TenantCode")
                .WithWebhookUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.Should().HaveCount(1);

                var subscriberConfiguration = result.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().BeNull();
                subscriberConfiguration.WebhookRequestRules.Count.Should().Be(1);

                var rule = subscriberConfiguration.WebhookRequestRules.Single();
                rule.Routes.Count.Should().Be(2);
                rule.Routes[0].Uri.Should().Be("https://order-{selector}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("*");
                rule.Routes[1].Uri.Should().Be("https://payments-{selector}.eshopworld.com/webhook/");
                rule.Routes[1].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[1].Selector.Should().Be("aSelector");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("selector").WhichValue.Should().Be("$.TenantCode");
                rule.Source.Replace.Should().ContainKey("orderCode").WhichValue.Should().Be("$.OrderCode");

                rule.Routes[0].AuthenticationConfig.Should().BeOfType<OidcAuthenticationConfig>();
                var authenticationConfig1 = rule.Routes[0].AuthenticationConfig as OidcAuthenticationConfig;
                authenticationConfig1.Uri.Should().Be("https://blah-blah.sts.eshopworld.com");
                authenticationConfig1.Scopes.Should().Contain(new[] { "scope1" });
                authenticationConfig1.Type.Should().Be(AuthenticationType.OIDC);
                authenticationConfig1.ClientId.Should().Be("captain-hook-id");

                rule.Routes[1].AuthenticationConfig.Should().BeOfType<OidcAuthenticationConfig>();
                var authenticationConfig2 = rule.Routes[1].AuthenticationConfig as OidcAuthenticationConfig;
                authenticationConfig2.Uri.Should().Be("https://blah-blah.sts.eshopworld.com");
                authenticationConfig2.Scopes.Should().Contain(new[] { "scope1" });
                authenticationConfig2.Type.Should().Be(AuthenticationType.OIDC);
                authenticationConfig2.ClientId.Should().Be("captain-hook-id");
            }
        }

        [Theory, IsUnit]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("PUT")]
        public async Task MapSubscriberAsync_WithMultipleWebhooksAndNoDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb)
        {
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
               "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhookSelectionRule("$.TenantCode")
                .WithWebhookUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "bSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.Should().HaveCount(1);

                var subscriberConfiguration = result.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().BeNull();
                subscriberConfiguration.WebhookRequestRules.Count.Should().Be(1);

                var rule = subscriberConfiguration.WebhookRequestRules.Single();
                rule.Routes.Count.Should().Be(2);
                rule.Routes[0].Uri.Should().Be("https://order-{selector}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("aSelector");
                rule.Routes[1].Uri.Should().Be("https://payments-{selector}.eshopworld.com/webhook/");
                rule.Routes[1].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[1].Selector.Should().Be("bSelector");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("selector").WhichValue.Should().Be("$.TenantCode");
                rule.Source.Replace.Should().ContainKey("orderCode").WhichValue.Should().Be("$.OrderCode");

                rule.Routes[0].AuthenticationConfig.Should().BeOfType<OidcAuthenticationConfig>();
                var authenticationConfig1 = rule.Routes[0].AuthenticationConfig as OidcAuthenticationConfig;
                authenticationConfig1.Uri.Should().Be("https://blah-blah.sts.eshopworld.com");
                authenticationConfig1.Scopes.Should().Contain(new[] { "scope1" });
                authenticationConfig1.Type.Should().Be(AuthenticationType.OIDC);
                authenticationConfig1.ClientId.Should().Be("captain-hook-id");

                rule.Routes[1].AuthenticationConfig.Should().BeOfType<OidcAuthenticationConfig>();
                var authenticationConfig2 = rule.Routes[1].AuthenticationConfig as OidcAuthenticationConfig;
                authenticationConfig2.Uri.Should().Be("https://blah-blah.sts.eshopworld.com");
                authenticationConfig2.Scopes.Should().Contain(new[] { "scope1" });
                authenticationConfig2.Type.Should().Be(AuthenticationType.OIDC);
                authenticationConfig2.ClientId.Should().Be("captain-hook-id");
            }
        }

        [Theory, IsUnit]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("PUT")]
        public async Task MapSubscriberAsync_WithMultipleWebhooksAndSelectorInUriTransformDefined_MapsToRouteAndReplace(string httpVerb)
        {
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
               "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhookSelectionRule("$.TenantCode")
                .WithWebhookUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.Should().HaveCount(1);

                var subscriberConfiguration = result.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().BeNull();
                subscriberConfiguration.WebhookRequestRules.Count.Should().Be(1);

                var rule = subscriberConfiguration.WebhookRequestRules.Single();
                rule.Routes.Count.Should().Be(2);
                rule.Routes[0].Uri.Should().Be("https://order-{selector}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("*");
                rule.Routes[1].Uri.Should().Be("https://payments-{selector}.eshopworld.com/webhook/");
                rule.Routes[1].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[1].Selector.Should().Be("aSelector");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("selector").WhichValue.Should().Be("$.TenantCode");
                rule.Source.Replace.Should().ContainKey("orderCode").WhichValue.Should().Be("$.OrderCode");

                rule.Routes[0].AuthenticationConfig.Should().BeOfType<OidcAuthenticationConfig>();
                var authenticationConfig1 = rule.Routes[0].AuthenticationConfig as OidcAuthenticationConfig;
                authenticationConfig1.Uri.Should().Be("https://blah-blah.sts.eshopworld.com");
                authenticationConfig1.Scopes.Should().Contain(new[] { "scope1" });
                authenticationConfig1.Type.Should().Be(AuthenticationType.OIDC);
                authenticationConfig1.ClientId.Should().Be("captain-hook-id");

                rule.Routes[1].AuthenticationConfig.Should().BeOfType<OidcAuthenticationConfig>();
                var authenticationConfig2 = rule.Routes[1].AuthenticationConfig as OidcAuthenticationConfig;
                authenticationConfig2.Uri.Should().Be("https://blah-blah.sts.eshopworld.com");
                authenticationConfig2.Scopes.Should().Contain(new[] { "scope1" });
                authenticationConfig2.Type.Should().Be(AuthenticationType.OIDC);
                authenticationConfig2.ClientId.Should().Be("captain-hook-id");
            }
        }

        [Theory, IsUnit]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("PUT")]
        public async Task MapSubscriberAsync_WithSingleWebhookAndInvalidUriTransform_MapsToRouteAndReplace(string httpVerb)
        {
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
               "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var uriTransform = new UriTransformEntity(null);
            var subscriber = new SubscriberBuilder()
                .WithWebhookSelectionRule("aSelectionRule")
                .WithWebhookUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.Should().HaveCount(1);
                var subscriberConfiguration = result.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().BeNull();
                subscriberConfiguration.WebhookRequestRules.Count.Should().Be(1);
                var rule = subscriberConfiguration.WebhookRequestRules.Single();
                rule.Routes.Count.Should().Be(1);
                rule.Routes[0].Uri.Should().Be("https://blah-{orderCode}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("*");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("selector").WhichValue.Should().Be("aSelectionRule");

                rule.Routes[0].AuthenticationConfig.Should().BeOfType<OidcAuthenticationConfig>();
                var authenticationConfig = rule.Routes[0].AuthenticationConfig as OidcAuthenticationConfig;
                authenticationConfig.Uri.Should().Be("https://blah-blah.sts.eshopworld.com");
                authenticationConfig.Scopes.Should().Contain(new[] { "scope1" });
                authenticationConfig.Type.Should().Be(AuthenticationType.OIDC);
                authenticationConfig.ClientId.Should().Be("captain-hook-id");
            }
        }

        [Fact, IsUnit]
        public async Task MapSubscriberAsync_NoSelectionRule_MapsFromFirstEndpoint()
        {
            // Arrange
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
                "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { {"selector", "$.Test" }});
            var subscriber = new SubscriberBuilder()
                .WithWebhookUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", "POST", null, authentication)
                .Create();

            // Act
            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            // Assert
            var webhookRequestRule = new WebhookRequestRule
            {
                Source = new SourceParserLocation
                {
                    Replace = new Dictionary<string, string>
                    {
                        { "selector", "$.Test" }
                    }
                }
            };

            result.Should().HaveCount(1)
                .And.Subject.First().WebhookRequestRules.Should().HaveCount(1)
                .And.Subject.First().Should().BeEquivalentTo(
                    webhookRequestRule,
                    opt => opt.Including(x => x.Source.Replace));

        }
    }
}