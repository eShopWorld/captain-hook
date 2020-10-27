using System;
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
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Infrastructure.SubscriberEntityToConfigurationMapperTests
{
    public class MapToWebhookAsyncTests
    {
        private readonly Mock<ISecretProvider> _secretProviderMock = new Mock<ISecretProvider>();

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

        public MapToWebhookAsyncTests()
        {
            _secretProviderMock.Setup(m => m.GetSecretValueAsync("kv-secret-name")).ReturnsAsync(MapSubscriberAsyncTestData.Secret);
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenSingleWebhookAndNoUriTransformDefined_MapsToSingleWebhook(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication: authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                var subscriberConfiguration = result.Data;
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().Be("https://blah-blah.eshopworld.com/webhook/");
                subscriberConfiguration.HttpMethod.Should().Be(new HttpMethod(httpVerb));

                subscriberConfiguration.AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenSingleCallbackAndNoUriTransformDefined_MapsToSingleCallback(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication: authentication)
                .WithCallback("https://blah-blah.eshopworld.com/webhook/", httpVerb, "*", authentication: authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                var subscriberConfiguration = result.Data;
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
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenSingleWebhookAndNoSelectionRuleAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                var subscriberConfiguration = result.Data;
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().BeNull();

                var rules = subscriberConfiguration.WebhookRequestRules.Where(x => x.Routes.Any());
                rules.Should().HaveCount(1);
                var rule = rules.Single();
                rule.Routes.Should().HaveCount(1);
                rule.Routes[0].Uri.Should().Be("https://blah-{orderCode}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("*");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("orderCode").WhichValue.Should().Be("$.OrderCode");

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenSingleCallbackAndNoSelectionRuleAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithCallbacksUriTransform(uriTransform)
                .WithCallback("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                var subscriberConfiguration = result.Data;
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
                rule.Routes.Should().HaveCount(1);
                rule.Routes[0].Uri.Should().Be("https://blah-{orderCode}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("*");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("orderCode").WhichValue.Should().Be("$.OrderCode");

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenMultipleWebhooksAndDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("$.TenantCode")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                var subscriberConfiguration = result.Data;
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().BeNull();

                var rules = subscriberConfiguration.WebhookRequestRules.Where(x => x.Routes.Any());
                rules.Should().HaveCount(1);

                var rule = rules.Single();
                rule.Routes.Should().HaveCount(2);
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
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenMultipleCallbacksAndDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
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

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                var subscriberConfiguration = result.Data;
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Callback.WebhookRequestRules.Should()
                    .ContainEquivalentOf(ContentRequestRule).And
                    .ContainEquivalentOf(StatusCodeRequestRule).And
                    .HaveCount(3);

                var rule = subscriberConfiguration.Callback.WebhookRequestRules.SingleOrDefault(x => x.Destination?.RuleAction == RuleAction.RouteAndReplace);
                rule.Should().NotBeNull();
                rule.Routes.Should().HaveCount(2);
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
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenMultipleWebhooksAndNoDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("$.TenantCode")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "bSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                var subscriberConfiguration = result.Data;
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().BeNull();

                var rules = subscriberConfiguration.WebhookRequestRules.Where(x => x.Routes.Any());
                rules.Should().HaveCount(1);

                var rule = rules.Single();
                rule.Routes.Should().HaveCount(2);
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
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenMultipleCallbacksAndNoDefaultSelectorAndUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
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

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                var subscriberConfiguration = result.Data;
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
                rule.Routes.Should().HaveCount(2);
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
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenMultipleWebhooksAndSelectorInUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(
                new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("$.TenantCode")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                var subscriberConfiguration = result.Data;
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().BeNull();

                var rules = subscriberConfiguration.WebhookRequestRules.Where(x => x.Routes.Any());
                rules.Should().HaveCount(1);

                var rule = rules.Single();
                rule.Routes.Should().HaveCount(2);
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
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenMultipleCallbacksAndSelectorInUriTransformDefined_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
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

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                var subscriberConfiguration = result.Data;
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Callback.WebhookRequestRules.Should()
                    .ContainEquivalentOf(ContentRequestRule).And
                    .ContainEquivalentOf(StatusCodeRequestRule).And
                    .HaveCount(3);

                var rule = subscriberConfiguration.Callback.WebhookRequestRules.SingleOrDefault(x => x.Destination?.RuleAction == RuleAction.RouteAndReplace);
                rule.Should().NotBeNull();
                rule.Routes.Should().HaveCount(2);
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
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenSingleWebhookAndInvalidUriTransform_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("aSelectionRule")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                var subscriberConfiguration = result.Data;
                subscriberConfiguration.Should().NotBeNull();
                subscriberConfiguration.SubscriberName.Should().Be("captain-hook");
                subscriberConfiguration.EventType.Should().Be("event");
                subscriberConfiguration.Uri.Should().BeNull();
                var rules = subscriberConfiguration.WebhookRequestRules.Where(x => x.Routes.Any());
                rules.Should().HaveCount(1);
                var rule = rules.Single();
                rule.Routes.Should().HaveCount(1);
                rule.Routes[0].Uri.Should().Be("https://blah-{orderCode}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("*");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("selector").WhichValue.Should().Be("aSelectionRule");

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenSingleCallbackAndInvalidUriTransform_MapsToRouteAndReplace(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode" });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksSelectionRule("aSelectionRule")
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .WithCallbacksSelectionRule("aSelectionRule")
                .WithCallbacksUriTransform(uriTransform)
                .WithCallback("https://blah-{orderCode}.eshopworld.com/webhook/", httpVerb, "*", authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();

                var subscriberConfiguration = result.Data;
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
                rule.Routes.Should().HaveCount(1);
                rule.Routes[0].Uri.Should().Be("https://blah-{orderCode}.eshopworld.com/webhook/");
                rule.Routes[0].HttpMethod.Should().Be(new HttpMethod(httpVerb));
                rule.Routes[0].Selector.Should().Be("*");
                rule.Destination.RuleAction.Should().Be(RuleAction.RouteAndReplace);
                rule.Source.Replace.Should().ContainKey("selector").WhichValue.Should().Be("aSelectionRule");

                rule.Routes[0].AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
            }
        }

        [Theory, IsUnit]
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenNoSelectionRule_MapsFromFirstEndpoint(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { { "selector", "$.Test" } });
            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://blah-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .Create();

            // Act
            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

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
                var subscriberConfiguration = result.Data;
                subscriberConfiguration.AuthenticationConfig.Should().BeValidConfiguration(expectedAuthenticationConfig);
                var rules = subscriberConfiguration.WebhookRequestRules.Where(x => x.Routes.Any());
                rules.Should().HaveCount(1);
                rules.Single().Should().BeEquivalentTo(webhookRequestRule, opt => opt.Including(x => x.Source.Replace));
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleWebhookHasDefinedRetrySleepDurations_MapsRetrySleepDurations()
        {
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var webhooksRetrySleepDurations = new[] { TimeSpan.FromSeconds(11), TimeSpan.FromSeconds(22) };
            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://order-abc.eshopworld.com/webhook/", "PUT", null, authentication, webhooksRetrySleepDurations)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.RetrySleepDurations.Should().BeEquivalentTo(webhooksRetrySleepDurations);
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleWebhookAndSingleCallbackHaveDefinedRetrySleepDurations_MapsRetrySleepDurations()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var webhooksRetrySleepDurations = new[] {TimeSpan.FromSeconds(11), TimeSpan.FromSeconds(22)};
            var callbacksRetrySleepDuration = new[] {TimeSpan.FromSeconds(33), TimeSpan.FromSeconds(44)};

            var subscriber = new SubscriberBuilder()
               .WithWebhooksUriTransform(uriTransform)
               .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication, webhooksRetrySleepDurations)
               .WithCallbacksUriTransform(uriTransform)
               .WithCallback("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication, callbacksRetrySleepDuration)
               .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes
                    .First()
                    .RetrySleepDurations.Should().BeEquivalentTo(webhooksRetrySleepDurations);
                result.Data.Callback
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes
                    .First()
                    .RetrySleepDurations.Should().BeEquivalentTo(callbacksRetrySleepDuration);
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleWebhookAndSingleCallbackHaveNotDefinedRetrySleepDurations_MapsDefaultSleepDurations()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var subscriber = new SubscriberBuilder()
               .WithWebhooksUriTransform(uriTransform)
               .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
               .WithCallbacksUriTransform(uriTransform)
               .WithCallback("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
               .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes
                    .First()
                    .RetrySleepDurations.Should().BeEquivalentTo(new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) });
                result.Data.Callback
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes
                    .First()
                    .RetrySleepDurations.Should().BeEquivalentTo(new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) });
            }
        }

        [Fact, IsUnit]
        public async Task WhenManyWebhooksAndManyCallbacksHaveDefinedRetrySleepDurations_MapsRetrySleepDurations()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var webhooksRetrySleepDurations = new[] { TimeSpan.FromSeconds(11), TimeSpan.FromSeconds(22) };
            var callbacksRetrySleepDurations = new[] { TimeSpan.FromSeconds(12), TimeSpan.FromSeconds(23) };

            var subscriber = new SubscriberBuilder()
               .WithWebhooksSelectionRule("$.TenantCode")
               .WithWebhooksUriTransform(uriTransform)
               .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication, webhooksRetrySleepDurations)
               .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication, webhooksRetrySleepDurations)
               .WithCallbacksSelectionRule("$.TenantCode")
               .WithCallbacksUriTransform(uriTransform)
               .WithCallback("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication, callbacksRetrySleepDurations)
               .WithCallback("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication, callbacksRetrySleepDurations)
               .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                var webhookRoutes = result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                webhookRoutes[0].RetrySleepDurations.Should().BeEquivalentTo(webhooksRetrySleepDurations);
                webhookRoutes[1].RetrySleepDurations.Should().BeEquivalentTo(webhooksRetrySleepDurations);
                var callbackRoutes = result.Data
                    .Callback
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                callbackRoutes[0].RetrySleepDurations.Should().BeEquivalentTo(callbacksRetrySleepDurations);
                callbackRoutes[1].RetrySleepDurations.Should().BeEquivalentTo(callbacksRetrySleepDurations);
            }
        }

        [Fact, IsUnit]
        public async Task WhenManyWebhooksAndManyCallbacksHaveNotDefinedRetrySleepDurations_MapsDefaultRetrySleepDurations()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

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

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                var webhookRoutes = result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                webhookRoutes[0].RetrySleepDurations.Should().BeEquivalentTo(new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) });
                webhookRoutes[1].RetrySleepDurations.Should().BeEquivalentTo(new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) });
                var callbackRoutes = result.Data
                    .Callback
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                callbackRoutes[0].RetrySleepDurations.Should().BeEquivalentTo(new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) });
                callbackRoutes[1].RetrySleepDurations.Should().BeEquivalentTo(new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) });
            }
        }

        [Fact]
        [IsUnit]
        public async Task WhenInvalidSecretKey_ReturnsError()
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
            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            // Assert
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
            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            // Assert
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
                .WithCallback("https://blah-{selector}.eshopworld.com/dlq/", "POST", "*", authenticationEntity)
                .Create();

            // Act
            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            // Assert
            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.WebhookRequestRules.Should().HaveCount(1);
                result.Data.WebhookRequestRules.Single().Should().BeEquivalentTo(RequestRule);
                result.Data.Callback.WebhookRequestRules.Should().HaveCount(2);
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleWebhookAndSingleCallbackHaveNotDefinedTimeout_MapsDefaultTimeout()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithCallbacksUriTransform(uriTransform)
                .WithCallback("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes
                    .First()
                    .Timeout.Should().Be(TimeSpan.FromSeconds(100));
                result.Data.Callback
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes
                    .First()
                    .Timeout.Should().Be(TimeSpan.FromSeconds(100));
            }
        }

        [Fact, IsUnit]
        public async Task WhenManyWebhooksAndManyCallbacksHaveNotDefinedTimeout_MapsDefaultTimeout()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

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

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                var webhookRoutes = result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                webhookRoutes[0].Timeout.Should().Be(TimeSpan.FromSeconds(100));
                webhookRoutes[1].Timeout.Should().Be(TimeSpan.FromSeconds(100));
                var callbackRoutes = result.Data
                    .Callback
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                callbackRoutes[0].Timeout.Should().Be(TimeSpan.FromSeconds(100));
                callbackRoutes[1].Timeout.Should().Be(TimeSpan.FromSeconds(100));
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleWebhookHasDefinedTimeout_MapsTimeout()
        {
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");

            var webhooksTimeout = TimeSpan.FromSeconds(11);
            var callbacksTimeout = TimeSpan.FromSeconds(33);

            var subscriber = new SubscriberBuilder()
               .WithWebhook("https://order-abc.eshopworld.com/webhook/", "PUT", null, authentication, timeout: webhooksTimeout)
               .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Timeout.Should().Be(webhooksTimeout);
            }
        }

        [Fact, IsUnit]
        public async Task WhenManyWebhooksAndManyCallbacksHaveDefinedTimeout_MapsTimeout()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var webhooksTimeout = TimeSpan.FromSeconds(11);
            var callbacksTimeout = TimeSpan.FromSeconds(12);

            var subscriber = new SubscriberBuilder()
               .WithWebhooksSelectionRule("$.TenantCode")
               .WithWebhooksUriTransform(uriTransform)
               .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication, timeout: webhooksTimeout)
               .WithWebhook("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication, timeout: webhooksTimeout)
               .WithCallbacksSelectionRule("$.TenantCode")
               .WithCallbacksUriTransform(uriTransform)
               .WithCallback("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication, timeout: callbacksTimeout)
               .WithCallback("https://payments-{selector}.eshopworld.com/webhook/", httpVerb, "aSelector", authentication, timeout: callbacksTimeout)
               .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                var webhookRoutes = result.Data
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                webhookRoutes[0].Timeout.Should().Be(webhooksTimeout);
                webhookRoutes[1].Timeout.Should().Be(webhooksTimeout);
                var callbackRoutes = result.Data
                    .Callback
                    .WebhookRequestRules
                    .Single(x => x.Routes.Any())
                    .Routes;
                callbackRoutes[0].Timeout.Should().Be(callbacksTimeout);
                callbackRoutes[1].Timeout.Should().Be(callbacksTimeout);
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleSimpleWebhookHasDefinedMaxDeliveryCount_MapsMaxDeliveryCount()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");

            var subscriber = new SubscriberBuilder()
                .WithWebhook("https://order-test.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithMaxDeliveryCount(20)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.MaxDeliveryCount.Should().Be(20);
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleRoutedWebhookHasDefinedMaxDeliveryCount_MapsMaxDeliveryCount()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .WithMaxDeliveryCount(20)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.MaxDeliveryCount.Should().Be(20);
            }
        }

        [Fact, IsUnit]
        public async Task WhenSingleWebhookHasNotDefinedMaxDeliveryCount_MapsDefaultMaxDeliveryCount()
        {
            const string httpVerb = "PUT";
            var authentication = new BasicAuthenticationEntity("mark", "kv-secret-name");
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { ["orderCode"] = "$.OrderCode", ["selector"] = "$.TenantCode" });

            var subscriber = new SubscriberBuilder()
                .WithWebhooksUriTransform(uriTransform)
                .WithWebhook("https://order-{selector}.eshopworld.com/webhook/", httpVerb, null, authentication)
                .Create();

            var result = await new SubscriberEntityToConfigurationMapper(_secretProviderMock.Object).MapToWebhookAsync(subscriber);

            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.MaxDeliveryCount.Should().Be(10);
            }
        }
    }
}