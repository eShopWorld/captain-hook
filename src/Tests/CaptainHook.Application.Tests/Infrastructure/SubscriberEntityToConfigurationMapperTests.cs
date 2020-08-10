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

        [Fact, IsUnit]
        public async Task When_SingleWebhookDefined_Then_MappedCorrectlyOneRule()
        {
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
                "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "POST", string.Empty, authentication: authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            result.Should().HaveCount(1);
            var subscriberConfiguration = result.Single();
            subscriberConfiguration.Should().NotBeNull();
            subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
            subscriberConfiguration.EventType.Should().Be("event");
            subscriberConfiguration.Uri.Should().Be("https://blah-blah.eshopworld.com/webhook/");
        }

        [Fact, IsUnit]
        public async Task When_SingleWebhookWithNoSelectionRuleDefined_Then_MappedCorrectlyToRouteAndReplace()
        {
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
               "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", "POST", null, uriTransform, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

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
            rule.Routes[0].HttpMethod.Should().Be(HttpMethod.Post);
            rule.Routes[0].Selector.Should().Be("*");
            rule.Routes[0].AuthenticationConfig.Type.Should().Be(AuthenticationType.OIDC);
            rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
            rule.Source.Replace.Should().ContainKey("orderCode").WhichValue.Should().Be("$.OrderCode");
        }

        [Fact, IsUnit]
        public async Task When_MultipleWebhooksWithDefaultSelector_Then_MappedCorrectlyToRouteAndReplace()
        {
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
               "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhookSelectionRule("$.TenantCode")
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", "POST", null, uriTransform, authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", "POST", "aSelector", uriTransform, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);
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
            rule.Routes[0].HttpMethod.Should().Be(HttpMethod.Post);
            rule.Routes[0].Selector.Should().Be("*");
            rule.Routes[0].AuthenticationConfig.Type.Should().Be(AuthenticationType.OIDC);
            rule.Routes[1].Uri.Should().Be("https://payments-{selector}.eshopworld.com/webhook/");
            rule.Routes[1].HttpMethod.Should().Be(HttpMethod.Post);
            rule.Routes[1].Selector.Should().Be("aSelector");
            rule.Routes[1].AuthenticationConfig.Type.Should().Be(AuthenticationType.OIDC);
            rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
            rule.Source.Replace.Should().ContainKey("selector").WhichValue.Should().Be("$.TenantCode");
            rule.Source.Replace.Should().ContainKey("orderCode").WhichValue.Should().Be("$.OrderCode");
        }

        [Fact, IsUnit]
        public async Task When_MultipleWebhooksWithNoDefaultSelector_Then_MappedCorrectlyToRouteAndReplace()
        {
            var authentication = new AuthenticationEntity("captain-hook-id", new SecretStoreEntity("kvname", "kv-secret-name"),
               "https://blah-blah.sts.eshopworld.com", "OIDC", new[] { "scope1" });
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhookSelectionRule("$.TenantCode")
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", "POST", "aSelector", uriTransform, authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", "POST", "bSelector", uriTransform, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);
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
            rule.Routes[0].HttpMethod.Should().Be(HttpMethod.Post);
            rule.Routes[0].Selector.Should().Be("aSelector");
            rule.Routes[0].AuthenticationConfig.Type.Should().Be(AuthenticationType.OIDC);
            rule.Routes[1].Uri.Should().Be("https://payments-{selector}.eshopworld.com/webhook/");
            rule.Routes[1].HttpMethod.Should().Be(HttpMethod.Post);
            rule.Routes[1].Selector.Should().Be("bSelector");
            rule.Routes[1].AuthenticationConfig.Type.Should().Be(AuthenticationType.OIDC);
            rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
            rule.Source.Replace.Should().ContainKey("selector").WhichValue.Should().Be("$.TenantCode");
            rule.Source.Replace.Should().ContainKey("orderCode").WhichValue.Should().Be("$.OrderCode");
        }
    }
}