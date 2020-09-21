using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
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

namespace CaptainHook.Application.Tests.Infrastructure
{
    public class SubscriberEntityToConfigurationMapperTests
    {
        private readonly Mock<ISecretProvider> _secretProviderMock = new Mock<ISecretProvider>();

        private static readonly string Secret = "MySecret";

        private static readonly OidcAuthenticationEntity OidcAuthenticationEntity = new OidcAuthenticationEntity("captain-hook-id", "kv-secret-name", "https://blah-blah.sts.eshopworld.com", new[] { "scope1" });
        private static readonly BasicAuthenticationEntity BasicAuthenticationEntity = new BasicAuthenticationEntity("mark", "kv-secret-name");

        private static readonly OidcAuthenticationConfig OidcAuthenticationConfig = new OidcAuthenticationConfig { ClientId = "captain-hook-id", ClientSecret = Secret, Scopes = new[] { "scope1" }, Uri = "https://blah-blah.sts.eshopworld.com", Type = AuthenticationType.OIDC };
        private static readonly BasicAuthenticationConfig BasicAuthenticationConfig = new BasicAuthenticationConfig { Username = "mark", Password = Secret, Type = AuthenticationType.Basic };

        private static readonly WebhookRequestRule StatusCodeRequestRule = new WebhookRequestRule
        {
            Source = new SourceParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.HttpStatusCode
            },
            Destination = new ParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.Property,
                Path = "StatusCode"
            }
        };

        private static readonly WebhookRequestRule ContentRequestRule = new WebhookRequestRule
        {
            Source = new SourceParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.HttpContent
            },
            Destination = new ParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.String,
                Path = "Content"
            }
        };

        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { "POST", OidcAuthenticationEntity, OidcAuthenticationConfig },
                new object[] { "GET", OidcAuthenticationEntity, OidcAuthenticationConfig },
                new object[] { "PUT", OidcAuthenticationEntity, OidcAuthenticationConfig },
                new object[] { "POST", BasicAuthenticationEntity, BasicAuthenticationConfig },
                new object[] { "GET", BasicAuthenticationEntity, BasicAuthenticationConfig },
                new object[] { "PUT", BasicAuthenticationEntity, BasicAuthenticationConfig }
            };

        public SubscriberEntityToConfigurationMapperTests()
        {
            _secretProviderMock.Setup(m => m.GetSecretValueAsync("kv-secret-name")).ReturnsAsync(Secret);
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithSingleWebhookAndNoUriTransformDefined_MapsToSingleWebhook(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication: authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().Be("https://blah-blah.eshopworld.com/webhook/");
                subscriberConfiguration.HttpMethod.Should().Be(new HttpMethod(httpVerb));

                subscriberConfiguration.AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithSingleCallbackAndNoUriTransformDefined_MapsToSingleCallback(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication: authentication)
                .WithCallback("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication: authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");

                subscriberConfiguration.Callback.Uri.Should().Be("https://blah-blah.eshopworld.com/webhook/");
                subscriberConfiguration.Callback.HttpVerb.Should().Be(httpVerb);
                subscriberConfiguration.Callback.AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);

                subscriberConfiguration.Callback.WebhookRequestRules.Should()
                    .ContainEquivalentOf(ContentRequestRule).And
                    .ContainEquivalentOf(StatusCodeRequestRule).And
                    .HaveCount(2);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithSingleDLQAndNoUriTransformDefined_MapsToSingleDLQ(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .WithDlq("https://blah-blah.eshopworld.com/dlq/", httpVerb, "*", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

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
            var expected = new[] { webhookConfig, dlqConfig };

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(expected);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithSingleWebhookAndNoSelectionRuleAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
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

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithSingleCallbackAndNoSelectionRuleAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithCallbacksUriTransform(uriTransform)
                .WithCallback("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Callback.Uri.Should().BeNull();
                subscriberConfiguration.Callback.WebhookRequestRules.Should()
                    .ContainEquivalentOf(ContentRequestRule).And
                    .ContainEquivalentOf(StatusCodeRequestRule).And
                    .HaveCount(3);
                var rule = subscriberConfiguration.Callback.WebhookRequestRules.SingleOrDefault(x => x.Destination?.RuleAction == RuleAction.RouteAndReplace);
                rule.Should().NotBeNull();
                rule.Routes.Count.Should().Be(1);
                rule.Routes[0].Uri.Should().Be("https://blah-{orderCode}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("*");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("orderCode").WhichValue.Should().Be("$.OrderCode");

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithSingleDLQAndNoSelectionRuleAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithDlqhooksUriTransform(uriTransform)
                .WithDlq("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            var webhookConfig = new SubscriberConfigurationBuilder()
                .WithUri(null).WithoutAuthentication()
                .WithName(subscriber.Id)
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source
                        .AddReplace("orderCode", "$.OrderCode"))
                    .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                    .AddRoute(route => route
                        .WithHttpVerb(httpVerb)
                        .WithAuthentication(expectedAuthenticationConfig)
                        .WithUri("https://blah-{orderCode}.eshopworld.com/webhook/")
                        .WithSelector("*")))
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
                        .WithUri("https://blah-{orderCode}.eshopworld.com/webhook/")
                        .WithSelector("*")))
                .CreateAsDlq();
            var expected = new[] { webhookConfig, dlqConfig };

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().BeEquivalentTo(expected);
            }
        }


        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithMultipleWebhooksAndDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("$.TenantCode")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
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

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
                rule.Routes[1].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithMultipleCallbacksAndDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("$.TenantCode")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .WithCallbacksSelectionRule("$.TenantCode")
                .WithCallbacksUriTransform(uriTransform)
                .WithCallback("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithCallback("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Callback.WebhookRequestRules.Should()
                    .ContainEquivalentOf(ContentRequestRule).And
                    .ContainEquivalentOf(StatusCodeRequestRule).And
                    .HaveCount(3);

                var rule = subscriberConfiguration.Callback.WebhookRequestRules.SingleOrDefault(x => x.Destination?.RuleAction == RuleAction.RouteAndReplace);
                rule.Should().NotBeNull();
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

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
                rule.Routes[1].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithMultipleWebhooksAndNoDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("$.TenantCode")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "bSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
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

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);

                rule.Routes[1].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithMultipleCallbacksAndNoDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("$.TenantCode")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "bSelector", authentication)
                .WithCallbacksSelectionRule("$.TenantCode")
                .WithCallbacksUriTransform(uriTransform)
                .WithCallback("https://order-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .WithCallback("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "bSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Callback.WebhookRequestRules.Should()
                    .ContainEquivalentOf(ContentRequestRule).And
                    .ContainEquivalentOf(StatusCodeRequestRule).And
                    .HaveCount(3);
                subscriberConfiguration.Callback.Uri.Should().BeNull();

                var rule = subscriberConfiguration.Callback.WebhookRequestRules.SingleOrDefault(x => x.Destination?.RuleAction == RuleAction.RouteAndReplace);
                rule.Should().NotBeNull();
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

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);

                rule.Routes[1].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithMultipleWebhooksAndSelectorInUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("$.TenantCode")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
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

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
                rule.Routes[1].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithMultipleCallbacksAndSelectorInUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("$.TenantCode")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .WithCallbacksSelectionRule("$.TenantCode")
                .WithCallbacksUriTransform(uriTransform)
                .WithCallback("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithCallback("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Callback.WebhookRequestRules.Should()
                    .ContainEquivalentOf(ContentRequestRule).And
                    .ContainEquivalentOf(StatusCodeRequestRule).And
                    .HaveCount(3);

                var rule = subscriberConfiguration.Callback.WebhookRequestRules.SingleOrDefault(x => x.Destination?.RuleAction == RuleAction.RouteAndReplace);
                rule.Should().NotBeNull();
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

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
                rule.Routes[1].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithSingleWebhookAndInvalidUriTransform_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(null);
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("aSelectionRule")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
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

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_WithSingleCallbackAndInvalidUriTransform_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(null);
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("aSelectionRule")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .WithCallbacksSelectionRule("aSelectionRule")
                .WithCallbacksUriTransform(uriTransform)
                .WithCallback("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Callback.Uri.Should().BeNull();
                subscriberConfiguration.Callback.WebhookRequestRules.Should()
                    .ContainEquivalentOf(ContentRequestRule).And
                    .ContainEquivalentOf(StatusCodeRequestRule).And
                    .HaveCount(3);

                var rule = subscriberConfiguration.Callback.WebhookRequestRules.SingleOrDefault(x => x.Destination?.RuleAction == RuleAction.RouteAndReplace);
                rule.Should().NotBeNull();
                rule.Routes.Count.Should().Be(1);
                rule.Routes[0].Uri.Should().Be("https://blah-{orderCode}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("*");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("selector").WhichValue.Should().Be("aSelectionRule");

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task MapSubscriberAsync_NoSelectionRule_MapsFromFirstEndpoint(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { { "selector", "$.Test" } });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
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

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().HaveCount(1);
                var subscriberConfiguration = result.Data.Single();
                subscriberConfiguration.AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
                subscriberConfiguration.WebhookRequestRules.Should().HaveCount(1);
                subscriberConfiguration.WebhookRequestRules.First().Should().BeEquivalentTo(webhookRequestRule, opt => opt.Including(x => x.Source.Replace));
            }
        }

        [Fact]
        [IsUnit]
        public async Task MapSubscriberAsync_InvalidSecretKey_ReturnsError()
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { { "selector", "$.Test" } });
            var authentication = new OidcAuthenticationEntity("client-id", "invalid-secret-key-name", "uri", new string[] { });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", "POST", null, authentication)
                .Create();

            _secretProviderMock.Setup(x => x.GetSecretValueAsync("invalid-secret-key-name"))
                .Throws(new System.Exception());

            // Act
            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            // Assert
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
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { { "selector", "$.Test" } });
            var authentication = new BasicAuthenticationEntity("username", "password-key-name");
            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", "POST", "*", authentication)
                .Create();

            _secretProviderMock.Setup(x => x.GetSecretValueAsync("password-key-name"))
                .Throws(new System.Exception());

            // Act
            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapSubscriberAsync(subscriber);

            // Assert
            using (new AssertionScope())
            {
                result.IsError.Should().BeTrue();
                result.Error.Should().BeOfType(typeof(MappingError));
            }
        }
    }
}