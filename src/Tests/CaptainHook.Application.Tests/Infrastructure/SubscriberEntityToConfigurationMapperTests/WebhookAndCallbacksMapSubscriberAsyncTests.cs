﻿using System.Collections.Generic;
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
        [ClassData(typeof(MapSubscriberAsyncTestData))]
        public async Task WhenNoSelectionRule_MapsFromFirstEndpoint(string httpVerb, AuthenticationEntity authentication, AuthenticationConfig expectedAuthenticationConfig)
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { {"selector", "$.Test" }});
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
                subscriberConfiguration.WebhookRequestRules.Should().HaveCount(1);
                subscriberConfiguration.WebhookRequestRules.First().Should().BeEquivalentTo(webhookRequestRule, opt => opt.Including(x => x.Source.Replace));
            }
        }

        [Fact]
        [IsUnit]
        public async Task WhenInvalidSecretKey_ReturnsError()
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string> { { "selector", "$.Test" } });
            var authentication = new OidcAuthenticationEntity("client-id", "invalid-secret-key-name", "uri", new string[]{ });
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
    }
}