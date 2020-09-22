using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Infrastructure.SubscriberEntityToConfigurationMapperTests
{
    public class DlqhooksMappingTests
    {
        private readonly Mock<ISecretProvider> _secretProviderMock = new Mock<ISecretProvider>();

        public DlqhooksMappingTests()
        {
            _secretProviderMock.Setup(m => m.GetSecretValueAsync("kv-secret-name")).ReturnsAsync(MapSubscriberAsyncTestData.Secret);
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task MapSubscriberAsync_WithSingleDlqhookAndNoUriTransformDefined_MapsToSingleDlqhook(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", httpVerb, "*", authentication)
                .Create();

            var result = await new Application.Infrastructure.Mappers.SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            var webhookConfig = new SubscriberConfigurationBuilder()
                .WithName(subscriber.Id)
                .WithHttpVerb(httpVerb)
                .WithUri("https://blah-blah.eshopworld.com/webhook/")
                .WithAuthentication(expectedAuthenticationConfig)
                .Create();
            var dlqConfig = new SubscriberConfigurationBuilder()
                .WithHttpVerb(httpVerb)
                .WithUri("https://blah-blah.eshopworld.com/dlq/")
                .WithAuthentication(expectedAuthenticationConfig)
                .CreateAsDlq();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(webhookConfig, dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task MapSubscriberAsync_WithSingleDlqhookAndNoSelectionRuleAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://blah-{orderCode}.eshopworld.com/dlq/", httpVerb, null, authentication)
                .Create();

            var result = await new Application.Infrastructure.Mappers.SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            var webhookConfig = new SubscriberConfigurationBuilder()
                .WithName(subscriber.Id)
                .WithHttpVerb(httpVerb)
                .WithAuthentication(expectedAuthenticationConfig)
                .WithUri("https://blah-{orderCode}.eshopworld.com/webhook/")
                .Create();

            var dlqConfig = new SubscriberConfigurationBuilder()
                .WithUri(null).WithoutAuthentication()
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source
                        .AddReplace("orderCode", "$.OrderCode"))
                    .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                    .AddRoute(route => route
                        .WithHttpVerb(httpVerb)
                        .WithAuthentication(expectedAuthenticationConfig)
                        .WithUri("https://blah-{orderCode}.eshopworld.com/dlq/")
                        .WithSelector("*")))
                .CreateAsDlq();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(webhookConfig, dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task MapSubscriberAsync_WithMultipleDlqhooksAndDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksSelectionRule("$.TenantCode")
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://order-{selector}.eshopworld.com/dlq/", httpVerb, null, authentication)
                .WithDlqhook("https://payments-{selector}.eshopworld.com/dlq/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new Application.Infrastructure.Mappers.SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            var webhookConfig = new SubscriberConfigurationBuilder()
               .WithUri("https://blah-{selector}.eshopworld.com/webhook/")
               .WithAuthentication(expectedAuthenticationConfig)
               .WithName(subscriber.Id)
               .WithHttpVerb(httpVerb)
               .Create();

            var dlqConfig = new SubscriberConfigurationBuilder()
                .WithUri(null).WithoutAuthentication()
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source
                        .AddReplace("selector", "$.TenantCode")
                        .AddReplace("orderCode", "$.OrderCode"))
                    .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                    .AddRoute(route => route
                        .WithHttpVerb(httpVerb)
                        .WithAuthentication(expectedAuthenticationConfig)
                        .WithUri("https://order-{selector}.eshopworld.com/dlq/")
                        .WithSelector("*"))
                    .AddRoute(route => route
                        .WithHttpVerb(httpVerb)
                        .WithAuthentication(expectedAuthenticationConfig)
                        .WithUri("https://payments-{selector}.eshopworld.com/dlq/")
                        .WithSelector("aSelector")))
                .CreateAsDlq();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(webhookConfig, dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task MapSubscriberAsync_WithMultipleDlqhooksAndNoDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksSelectionRule("$.TenantCode")
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://order-{selector}.eshopworld.com/dlq/", httpVerb, "aSelector", authentication)
                .WithDlqhook("https://payments-{selector}.eshopworld.com/dlq/", httpVerb, "bSelector", authentication)
                .Create();

            var result = await new Application.Infrastructure.Mappers.SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            var webhookConfig = new SubscriberConfigurationBuilder()
                .WithUri("https://blah-{selector}.eshopworld.com/webhook/")
                .WithAuthentication(expectedAuthenticationConfig)
                .WithName(subscriber.Id)
                .WithHttpVerb(httpVerb)
                .Create();

            var dlqConfig = new SubscriberConfigurationBuilder()
                .WithUri(null).WithoutAuthentication()
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source
                        .AddReplace("selector", "$.TenantCode")
                        .AddReplace("orderCode", "$.OrderCode"))
                    .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                    .AddRoute(route => route
                        .WithHttpVerb(httpVerb)
                        .WithAuthentication(expectedAuthenticationConfig)
                        .WithUri("https://order-{selector}.eshopworld.com/dlq/")
                        .WithSelector("aSelector"))
                    .AddRoute(route => route
                        .WithHttpVerb(httpVerb)
                        .WithAuthentication(expectedAuthenticationConfig)
                        .WithUri("https://payments-{selector}.eshopworld.com/dlq/")
                        .WithSelector("bSelector")))
                .CreateAsDlq();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(webhookConfig, dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task MapSubscriberAsync_WithMultipleDlqhookAndSelectorInUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksSelectionRule("$.TenantCode")
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://order-{selector}.eshopworld.com/dlq/", httpVerb, null, authentication)
                .WithDlqhook("https://payments-{selector}.eshopworld.com/dlq/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new Application.Infrastructure.Mappers.SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            var webhookConfig = new SubscriberConfigurationBuilder()
               .WithUri("https://blah-{selector}.eshopworld.com/webhook/")
               .WithAuthentication(expectedAuthenticationConfig)
               .WithName(subscriber.Id)
               .WithHttpVerb(httpVerb)
               .Create();

            var dlqConfig = new SubscriberConfigurationBuilder()
               .WithUri(null).WithoutAuthentication()
               .AddWebhookRequestRule(rule => rule
                   .WithSource(source => source
                       .AddReplace("selector", "$.TenantCode")
                       .AddReplace("orderCode", "$.OrderCode"))
                   .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                   .AddRoute(route => route
                       .WithHttpVerb(httpVerb)
                       .WithAuthentication(expectedAuthenticationConfig)
                       .WithUri("https://order-{selector}.eshopworld.com/dlq/")
                       .WithSelector("*"))
                   .AddRoute(route => route
                       .WithHttpVerb(httpVerb)
                       .WithAuthentication(expectedAuthenticationConfig)
                       .WithUri("https://payments-{selector}.eshopworld.com/dlq/")
                       .WithSelector("aSelector")))
               .CreateAsDlq();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(webhookConfig, dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task MapSubscriberAsync_WithSingleDlqhookAndInvalidUriTransform_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(null);
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksSelectionRule("aSelectionRule")
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://blah-{orderCode}.eshopworld.com/dlq/", httpVerb, "*", authentication)
                .Create();

            var result = await new Application.Infrastructure.Mappers.SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            var webhookConfig = new SubscriberConfigurationBuilder()
                .WithUri("https://blah-{selector}.eshopworld.com/webhook/")
                .WithAuthentication(expectedAuthenticationConfig)
                .WithName(subscriber.Id)
                .WithHttpVerb(httpVerb)
                .Create();

            var dlqConfig = new SubscriberConfigurationBuilder()
               .WithUri(null).WithoutAuthentication()
               .AddWebhookRequestRule(rule => rule
                   .WithSource(source => source
                       .AddReplace("selector", "aSelectionRule"))
                   .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                   .AddRoute(route => route
                       .WithHttpVerb(httpVerb)
                       .WithAuthentication(expectedAuthenticationConfig)
                       .WithUri("https://blah-{orderCode}.eshopworld.com/dlq/")
                       .WithSelector("*")))
               .CreateAsDlq();


            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(webhookConfig, dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task MapSubscriberAsync_NoSelectionRule_MapsFromFirstEndpoint(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { { "selector", "$.Test" } });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://blah-{selector}.eshopworld.com/dlq/", httpVerb, "*", authentication)
                .Create();

            var result = await new Application.Infrastructure.Mappers.SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            var webhookConfig = new SubscriberConfigurationBuilder()
                .WithUri("https://blah-{selector}.eshopworld.com/webhook/")
                .WithAuthentication(expectedAuthenticationConfig)
                .WithName(subscriber.Id)
                .WithHttpVerb(httpVerb)
                .Create();

            var dlqConfig = new SubscriberConfigurationBuilder()
               .WithUri(null).WithoutAuthentication()
               .AddWebhookRequestRule(rule => rule
                   .WithSource(source => source
                       .AddReplace("selector", "$.Test"))
                   .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                   .AddRoute(route => route
                       .WithHttpVerb(httpVerb)
                       .WithAuthentication(expectedAuthenticationConfig)
                       .WithUri("https://blah-{selector}.eshopworld.com/dlq/")
                       .WithSelector("*")
                   ))
               .CreateAsDlq();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(webhookConfig, dlqConfig);
            }
        }

        [Fact]
        [IsUnit]
        public async Task MapSubscriberAsync_InvalidSecretKey_ReturnsError()
        {
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { { "selector", "$.Test" } });
            var authentication = new OidcAuthenticationEntity("client-id", "invalid-secret-key-name", "uri", new string[] { });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", "POST", null)
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://blah-{selector}.eshopworld.com/dlq/", "POST", "*", authentication)
                .Create();

            _secretProviderMock.Setup(x => x.GetSecretValueAsync("invalid-secret-key-name"))
                .Throws(new System.Exception());

            var result = await new Application.Infrastructure.Mappers.SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeTrue();
                result.Error.Should().BeOfType(typeof(MappingError));
            }
        }

        [Fact]
        [IsUnit]
        public async Task MapSubscriberAsync_InvalidPasswordKey_ReturnsError()
        {
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { { "selector", "$.Test" } });
            var authentication = new BasicAuthenticationEntity("username", "password-key-name");
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", "POST", null)
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://blah-{selector}.eshopworld.com/dlq/", "POST", "*", authentication)
                .Create();

            _secretProviderMock.Setup(x => x.GetSecretValueAsync("password-key-name"))
                .Throws(new System.Exception());

            var result = await new Application.Infrastructure.Mappers.SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeTrue();
                result.Error.Should().BeOfType(typeof(MappingError));
            }
        }
    }
}