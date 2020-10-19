using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.TestsInfrastructure.Builders;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Infrastructure.SubscriberEntityToConfigurationMapperTests
{
    public class MapToDlqAsyncTests
    {
        private readonly Mock<ISecretProvider> _secretProviderMock = new Mock<ISecretProvider>();

        public MapToDlqAsyncTests()
        {
            _secretProviderMock.Setup(m => m.GetSecretValueAsync("kv-secret-name")).ReturnsAsync(MapSubscriberAsyncTestData.Secret);
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenSingleDlqhookAndNoUriTransformDefined_MapsToSingleDlqhook(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", httpVerb, "*", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            var dlqConfig = new SubscriberConfigurationBuilder()
                .WithHttpVerb(httpVerb)
                .WithUri("https://blah-blah.eshopworld.com/dlq/")
                .WithAuthentication(expectedAuthenticationConfig)
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source
                        .WithPath("$")
                        .WithType(DataType.Model))
                    .WithDestination(type: DataType.Model))
                .WithSubscriberName($"{subscriber.Name}-DLQ")
                .WithName(subscriber.Id)
                .WithSourceSubscriptionName(subscriber.Name)
                .AsDLQ()
                .Create();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenSingleDlqhookAndNoSelectionRuleAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://blah-{orderCode}.eshopworld.com/dlq/", httpVerb, null, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

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
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source
                        .WithPath("$")
                        .WithType(DataType.Model))
                   .WithDestination(type: DataType.Model))
                .WithSubscriberName($"{subscriber.Name}-DLQ")
                .WithName(subscriber.Id)
                .WithSourceSubscriptionName(subscriber.Name)
                .AsDLQ()
                .Create();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenMultipleDlqhooksAndDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
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

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

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
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source
                        .WithPath("$")
                        .WithType(DataType.Model))
                    .WithDestination(type: DataType.Model))
                .WithSubscriberName($"{subscriber.Name}-DLQ")
                .WithName(subscriber.Id)
                .WithSourceSubscriptionName(subscriber.Name)
                .AsDLQ()
                .Create();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenMultipleDlqhooksAndNoDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
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

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

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
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source
                        .WithPath("$")
                        .WithType(DataType.Model))
                    .WithDestination(type: DataType.Model))
                .WithSubscriberName($"{subscriber.Name}-DLQ")
                .WithName(subscriber.Id)
                .WithSourceSubscriptionName(subscriber.Name)
                .AsDLQ()
                .Create();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenMultipleDlqhookAndSelectorInUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
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

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

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
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source
                        .WithPath("$")
                        .WithType(DataType.Model))
                    .WithDestination(type: DataType.Model))
                .WithSubscriberName($"{subscriber.Name}-DLQ")
                .WithName(subscriber.Id)
                .WithSourceSubscriptionName(subscriber.Name)
                .AsDLQ()
                .Create();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(dlqConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenNoSelectionRule_MapsFromFirstEndpoint(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { { "selector", "$.Test" } });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://blah-{selector}.eshopworld.com/dlq/", httpVerb, "*", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

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
                        .WithSelector("*")))
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source
                        .WithPath("$")
                        .WithType(DataType.Model))
                    .WithDestination(type: DataType.Model))
                .WithSubscriberName($"{subscriber.Name}-DLQ")
                .WithName(subscriber.Id)
                .WithSourceSubscriptionName(subscriber.Name)
                .AsDLQ()
                .Create();

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(dlqConfig);
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleDlqhookHaveDefinedRetrySleepDurations_MapsRetrySleepDurations()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var retrySleepDurations = new [] { TimeSpan.FromSeconds(11), TimeSpan.FromSeconds(22) };
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", httpVerb, "*", authentication, retrySleepDurations)
                .WithDlqhooksUriTransform(uriTransform)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes
                    .First()
                    .RetrySleepDurations.Should().BeEquivalentTo(retrySleepDurations);
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleDlqhookHaveNotDefinedRetrySleepDurations_MapsDefaultRetrySleepDurations()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", httpVerb, "*", authentication)
                .WithDlqhooksUriTransform(uriTransform)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes
                    .First()
                    .RetrySleepDurations.Should().BeEquivalentTo(new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) });
            }
        }

        [Fact, IsUnit]
        public async Task WhenMultipleDlqhooksHaveDefinedRetrySleepDurations_MapsRetrySleepDurations()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var retrySleepDurations1 = new[] { TimeSpan.FromSeconds(11), TimeSpan.FromSeconds(22) };
            var retrySleepDurations2 = new[] { TimeSpan.FromSeconds(12), TimeSpan.FromSeconds(23) };

            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksSelectionRule("$.TenantCode")
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://order-{selector}.eshopworld.com/dlq/", httpVerb, null, authentication, retrySleepDurations1)
                .WithDlqhook("https://payments-{selector}.eshopworld.com/dlq/", httpVerb, "aSelector", authentication, retrySleepDurations2)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                var routes = result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                routes[0].RetrySleepDurations.Should().BeEquivalentTo(retrySleepDurations1);
                routes[1].RetrySleepDurations.Should().BeEquivalentTo(retrySleepDurations2);
            }
        }

        [Fact, IsUnit]
        public async Task WhenMultipleDlqhooksHaveNotDefinedRetrySleepDurations_MapsDefaultRetrySleepDurations()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksSelectionRule("$.TenantCode")
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://order-{selector}.eshopworld.com/dlq/", httpVerb, null, authentication)
                .WithDlqhook("https://payments-{selector}.eshopworld.com/dlq/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                var routes = result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                routes[0].RetrySleepDurations.Should().BeEquivalentTo(new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) });
                routes[1].RetrySleepDurations.Should().BeEquivalentTo(new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) });
            }
        }

        [Fact]
        [IsUnit]
        public async Task WhenInvalidSecretKey_ReturnsError()
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

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeTrue();
                result.Error.Should().BeOfType(typeof(MappingError));
            }
        }

        [Fact]
        [IsUnit]
        public async Task WhenInvalidPasswordKey_ReturnsError()
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

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeTrue();
                result.Error.Should().BeOfType(typeof(MappingError));
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(ValidPayloadTransforms))]
        public async Task WhenDifferentPayloadTransformsAreUsed_MapToCorrectRule(string payloadTransform)
        {
            // Arrange
            var RequestRule = new WebhookRequestRule
            {
                Source = new SourceParserLocation
                {
                    Path = payloadTransform,
                    Type = DataType.Model
                },
                Destination = new ParserLocation
                {
                    Type = DataType.Model
                }
            };

            var authenticationEntity = new BasicAuthenticationEntity("username", "kv-secret-name");

            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", "POST", "*", authenticationEntity)
                .WithWebhooksPayloadTransform(payloadTransform)
                .WithDlqhook("https://blah-{selector}.eshopworld.com/dlq/", "POST", "*", authenticationEntity)
                .WithDlqhooksPayloadTransform(payloadTransform)
                .Create();

            // Act
            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            // Assert
            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.WebhookRequestRules.Should().HaveCount(1);
                result.Data.WebhookRequestRules.Single().Should().BeEquivalentTo(RequestRule);
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleDlqhookHaveDefinedTimeout_MapsTimeout()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var timeout = TimeSpan.FromSeconds(11);
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", httpVerb, "*", authentication, timeout: timeout)
                .WithDlqhooksUriTransform(uriTransform)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes
                    .First()
                    .Timeout.Should().Be(timeout);
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleDlqhookHaveNotDefinedTimeout_MapsDefaultTimeout()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", httpVerb, "*", authentication)
                .WithDlqhooksUriTransform(uriTransform)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes
                    .First()
                    .Timeout.Should().Be(TimeSpan.FromSeconds(100));
            }
        }

        [Fact, IsUnit]
        public async Task WhenMultipleDlqhooksHaveDefinedTimeout_MapsTimeout()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var timeout1 = TimeSpan.FromSeconds(11);
            var timeout2 = TimeSpan.FromSeconds(12);

            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksSelectionRule("$.TenantCode")
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://order-{selector}.eshopworld.com/dlq/", httpVerb, null, authentication, timeout: timeout1)
                .WithDlqhook("https://payments-{selector}.eshopworld.com/dlq/", httpVerb, "aSelector", authentication, timeout: timeout2)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                var routes = result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                routes[0].Timeout.Should().Be(timeout1);
                routes[1].Timeout.Should().Be(timeout2);
            }
        }

        [Fact, IsUnit]
        public async Task WhenMultipleDlqhooksHaveNotDefinedTimeout_MapsDefaultTimeout()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksSelectionRule("$.TenantCode")
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlqhook("https://order-{selector}.eshopworld.com/dlq/", httpVerb, null, authentication)
                .WithDlqhook("https://payments-{selector}.eshopworld.com/dlq/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToDlqAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                var routes = result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                routes[0].Timeout.Should().Be(TimeSpan.FromSeconds(100));
                routes[1].Timeout.Should().Be(TimeSpan.FromSeconds(100));
            }
        }
    }
}