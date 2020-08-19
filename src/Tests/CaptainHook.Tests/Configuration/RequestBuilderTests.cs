using System;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Requests;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class RequestBuilderTests
    {
        [IsUnit]
        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public void IdempotencyKeyHeaderTests_Get(string method)
        {
            var config = new WebhookConfigBuilder()
                .WithHttpVerb(method)
                .WithType("eventType")
                .WithUri("https://blah.blah.eshopworld.com/webhook/")
                .WithOidcAuthentication()
                .AddWebhookRequestRule(rule => rule
                    .WithSource(source => source.WithPath("OrderCode"))
                    .WithDestination(location: Location.Uri))
                .Create();

            var messageData = new MessageData("blah", "blahtype", "blahsubscriber", "blahReplyTo", false) { ServiceBusMessageId = Guid.NewGuid().ToString(), CorrelationId = Guid.NewGuid().ToString() };

            var headers = new DefaultRequestBuilder(Mock.Of<IBigBrother>()).GetHttpHeaders(config, messageData);
            Assert.True(headers.RequestHeaders.ContainsKey(Constants.Headers.IdempotencyKey));
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(UriData))]
        public void UriConstructionTests(WebhookConfig config, string payload, string expectedUri)
        {
            var uri = new DefaultRequestBuilder(Mock.Of<IBigBrother>()).BuildUri(config, payload);

            Assert.Equal(new Uri(expectedUri), uri);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(WebhookConfigData))]
        public void SelectWebhookConfigTests(WebhookConfig config, string payload, WebhookConfig expectedWebhookConfig)
        {
            var actualConfig = new DefaultRequestBuilder(Mock.Of<IBigBrother>()).SelectWebhookConfig(config, payload);

            actualConfig.Should().BeEquivalentTo(expectedWebhookConfig);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(PayloadData))]
        public void PayloadConstructionTests(
            WebhookConfig config,
            string sourcePayload,
            Dictionary<string, object> data,
            string expectedPayload)
        {
            var requestPayload = new DefaultRequestBuilder(Mock.Of<IBigBrother>()).BuildPayload(config, sourcePayload, data);

            Assert.Equal(expectedPayload, requestPayload);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(HttpVerbData))]
        public void HttpVerbSelectionTests(
            WebhookConfig config,
            string sourcePayload,
            HttpMethod expectedVerb)
        {
            var selectedVerb = new DefaultRequestBuilder(Mock.Of<IBigBrother>()).SelectHttpMethod(config, sourcePayload);

            Assert.Equal(expectedVerb, selectedVerb);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(AuthenticationSchemeData))]
        public void AuthenticationSchemeSelectionTests(
            WebhookConfig config,
            string sourcePayload,
            AuthenticationType expectedAuthenticationType)
        {
            var authenticationConfig = new DefaultRequestBuilder(Mock.Of<IBigBrother>()).GetAuthenticationConfig(config, sourcePayload);

            Assert.Equal(expectedAuthenticationType, authenticationConfig.AuthenticationConfig.Type);
        }

        public static IEnumerable<object[]> UriData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .AddWebhookRequestRule(rules => rules
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    "https://blah.blah.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("BrandType"))
                            .WithDestination(ruleAction: RuleAction.Route)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand2.eshopworld.com/webhook")
                                .WithSelector("Brand2")
                                .WithNoAuthentication()))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderConfirmationRequestDto")))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    "https://blah.blah.brand1.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("BrandType"))
                            .WithDestination(ruleAction: RuleAction.Route)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand2.eshopworld.com/webhook")
                                .WithSelector("Brand2")
                                .WithNoAuthentication()))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderConfirmationRequestDto")))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\"}",
                    "https://blah.blah.brand2.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("BrandType"))
                            .WithDestination(ruleAction: RuleAction.Route)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand3.eshopworld.com/webhook")
                                .WithSelector("Brand2")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\"}",
                    "https://blah.blah.brand3.eshopworld.com/webhook"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("BrandType"))
                            .WithDestination(ruleAction: RuleAction.Route)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand2.eshopworld.com/webhook")
                                .WithSelector("Brand2")
                                .WithNoAuthentication()))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderConfirmationRequestDto")))
                        .Create(),
                    "{\"OrderCode\":\"DEV13:00026804\", \"BrandType\":\"Brand1\"}",
                    "https://blah.blah.brand1.eshopworld.com/webhook/DEV13%3A00026804"
                },
            };

        public static IEnumerable<object[]> WebhookConfigData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithType("Webhook1")
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .AddWebhookRequestRule(rules => rules
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .WithNoAuthentication()
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    new WebhookConfigBuilder()
                        .WithType("Webhook1")
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .AddWebhookRequestRule(rules => rules
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .WithNoAuthentication()
                        .Create()
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("BrandType"))
                            .WithDestination(ruleAction: RuleAction.Route)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand2.eshopworld.com/webhook")
                                .WithSelector("Brand2")
                                .WithNoAuthentication()))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderConfirmationRequestDto")))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    new WebhookConfigRouteBuilder()
                        .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                        .WithHttpVerb("POST")
                        .WithSelector("Brand1")
                        .WithNoAuthentication()
                        .Create()
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("BrandType"))
                            .WithDestination(ruleAction: RuleAction.Route)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand2.eshopworld.com/webhook")
                                .WithHttpVerb("PUT")
                                .WithSelector("Brand2")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\"}",
                    new WebhookConfigRouteBuilder()
                        .WithUri("https://blah.blah.brand2.eshopworld.com/webhook")
                        .WithHttpVerb("PUT")
                        .WithSelector("Brand2")
                        .WithNoAuthentication()
                        .Create()
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("BrandType"))
                            .WithDestination(ruleAction: RuleAction.Route)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand3.eshopworld.com/webhook")
                                .WithHttpVerb("PUT")
                                .WithSelector("Brand2")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\"}",
                    new WebhookConfigRouteBuilder()
                        .WithUri("https://blah.blah.brand3.eshopworld.com/webhook")
                        .WithHttpVerb("PUT")
                        .WithSelector("Brand2")
                        .WithNoAuthentication()
                        .Create()
                }
            };

        public static IEnumerable<object[]> PayloadData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("InnerModel"))
                            .WithDestination(type: DataType.Model, ruleAction: RuleAction.Replace))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, object> (),
                    "{\"Msg\":\"Buy this thing\"}"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("InnerModel"))
                            .WithDestination(type: DataType.Model, path: "Payload"))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(type: DataType.Model, path: "OrderCode"))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, object>(),
                    "{\"Payload\":{\"Msg\":\"Buy this thing\"},\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\"}"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(path: "OrderCode"))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithType(DataType.HttpStatusCode).WithLocation(Location.HttpStatusCode))
                            .WithDestination(type: DataType.Property, path: "HttpStatusCode"))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithType(DataType.HttpContent))
                            .WithDestination(type: DataType.Model, path: "Content"))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, object>{{"HttpStatusCode", 200}, {"HttpResponseContent", "{\"Msg\":\"Buy this thing\"}" } },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\",\"HttpStatusCode\":200,\"Content\":{\"Msg\":\"Buy this thing\"}}"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("InnerModel"))
                            .WithDestination(type: DataType.Model))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, object>(),
                    "{\"Msg\":\"Buy this thing\"}"
                },
            };

        public static IEnumerable<object[]> HttpVerbData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    HttpMethod.Post
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("BrandType"))
                            .WithDestination(ruleAction: RuleAction.Route)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                                .WithHttpVerb("POST")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand2.eshopworld.com/webhook")
                                .WithHttpVerb("PUT")
                                .WithSelector("Brand2")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\"}",
                    HttpMethod.Put
                }
            };

        public static IEnumerable<object[]> AuthenticationSchemeData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .WithNoAuthentication()
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    AuthenticationType.None
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .WithOidcAuthentication()
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    AuthenticationType.OIDC
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .WithOidcAuthentication()
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("BrandType"))
                            .WithDestination(ruleAction: RuleAction.Route)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand2.eshopworld.com/webhook")
                                .WithSelector("Brand2")
                                .WithBasicAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\"}",
                    AuthenticationType.Basic
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .WithUri("https://blah.blah.eshopworld.com/webhook/")
                        .WithOidcAuthentication()
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("OrderCode"))
                            .WithDestination(location: Location.Uri))
                        .AddWebhookRequestRule(rule => rule
                            .WithSource(source => source.WithPath("BrandType"))
                            .WithDestination(ruleAction: RuleAction.Route)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand1.eshopworld.com/webhook")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.brand2.eshopworld.com/webhook")
                                .WithSelector("Brand2")
                                .WithBasicAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    AuthenticationType.None
                }
            };   
    }
}
