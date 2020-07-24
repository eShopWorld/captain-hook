using System;
using System.Collections.Generic;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry.Message;
using CaptainHook.EventHandlerActor.Handlers.Requests;
using CaptainHook.Tests.Builders;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Services.Actors.Requests
{
    public class RouteAndReplaceRequestBuilderTests
    {
        private readonly Mock<IValidator<WebhookConfig>> _validatorMock = new Mock<IValidator<WebhookConfig>>();

        private readonly Mock<IBigBrother> _bigBrotherMock = new Mock<IBigBrother>();

        private readonly RouteAndReplaceRequestBuilder _builder;

        public RouteAndReplaceRequestBuilderTests()
        {
            _builder = new RouteAndReplaceRequestBuilder(_bigBrotherMock.Object, _validatorMock.Object);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(UriDataRouteAndReplace))]
        public void UriConstructionRouteAndReplaceTests(WebhookConfig config, string payload, string expectedUri)
        {
            _validatorMock.Setup(v => v.Validate(It.IsAny<WebhookConfig>())).Returns(new ValidationResult());

            var uri = _builder.BuildUri(config, payload);

            uri.Should().BeEquivalentTo(new Uri(expectedUri));
        }

        [IsUnit]
        [Fact]
        public void BuildUri_PayloadDoesNotContainReplaceValues_UnroutablePublishedAndNullReturned()
        {
            var config = new WebhookConfigBuilder()
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                    .WithSource(source => source
                        .AddReplace("selector", "$.TenantCode")
                        .AddReplace("orderCode", "$.OrderCode"))
                    .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                    .AddRoute(route => route
                        .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                        .WithSelector("*")
                        .WithNoAuthentication()))
                .Create();
            const string payload = "{\"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}";
            _validatorMock.Setup(v => v.Validate(It.IsAny<WebhookConfig>())).Returns(new ValidationResult());

            var uri = _builder.BuildUri(config, payload);

            uri.Should().BeNull();
            _bigBrotherMock.Verify(bb => bb.Publish(
                    It.Is<UnroutableMessageEvent>(
                        message => message.Message == "Error looking for $.OrderCode in the message payload"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Once);
        }

        [IsUnit]
        [Fact]
        public void BuildUri_PayloadDoesNotContainAnyReplaceValues_UnroutablePublishedAndNullReturned()
        {
            var config = new WebhookConfigBuilder()
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                    .WithSource(source => source
                        .AddReplace("selector", "$.TenantCode")
                        .AddReplace("orderCode", "$.OrderCode"))
                    .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                    .AddRoute(route => route
                        .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                        .WithSelector("*")
                        .WithNoAuthentication()))
                .Create();
            const string payload = "{\"BrandType\":\"Brand2\"}";
            _validatorMock.Setup(v => v.Validate(It.IsAny<WebhookConfig>())).Returns(new ValidationResult());

            var uri = _builder.BuildUri(config, payload);

            uri.Should().BeNull();
            _bigBrotherMock.Verify(bb => bb.Publish(
                    It.Is<UnroutableMessageEvent>(
                        message => message.Message == "Error looking for $.TenantCode in the message payload"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Exactly(2)); // 2 times because 1 for selector and the other for replacement dictionary
            _bigBrotherMock.Verify(bb => bb.Publish(
                    It.Is<UnroutableMessageEvent>(
                        message => message.Message == "Error looking for $.OrderCode in the message payload"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Once); // 1 time only for replacement dictionary
        }

        [IsUnit]
        [Fact]
        public void BuildUri_InvalidConfig_ReturnsNull()
        {
            var validationResult = new ValidationResult(new[] { new ValidationFailure("a", "error") });
            _validatorMock.Setup(v => v.Validate(It.IsAny<WebhookConfig>())).Returns(validationResult);

            var uri = _builder.BuildUri(new WebhookConfig(), "payload-payload");

            uri.Should().BeNull("because failure to validate should result in null uri");
        }

        [IsUnit]
        [Fact]
        public void BuildUri_InvalidConfig_PublishesMessage()
        {
            var validationResult = new ValidationResult(new[] { new ValidationFailure("a", "error") });
            _validatorMock.Setup(v => v.Validate(It.IsAny<WebhookConfig>())).Returns(validationResult);
            
            _builder.BuildUri(new WebhookConfigBuilder().WithType("type-for-test").Create(), "payload-payload");

            _bigBrotherMock.Verify(bb => bb.Publish(
                    It.Is<UnroutableMessageEvent>(
                        message => message.EventType == "type-for-test" &&
                                   message.SubscriberName == "type-for-test" &&
                                   message.Message == "Validation errors for subscriber configuration: error"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Once);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(WebhookConfigData))]
        public void SelectWebhookConfigTests(WebhookConfig config, string payload, WebhookConfig expectedWebhookConfig)
        {
            var actualConfig = _builder.SelectWebhookConfig(config, payload);

            actualConfig.Should().BeEquivalentTo(expectedWebhookConfig);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(WebhookConfigData))]
        public void GetAuthenticationConfigTests(WebhookConfig config, string payload, WebhookConfig expectedWebhookConfig)
        {
            var actualConfig = _builder.GetAuthenticationConfig(config, payload);

            actualConfig.Should().BeEquivalentTo(expectedWebhookConfig);
        }


        [IsUnit]
        [Theory]
        [MemberData(nameof(WebhookConfigData))]
        public void SelectHttpMethodTests(WebhookConfig config, string payload, WebhookConfig expectedWebhookConfig)
        {
            var actualHttpMethod = _builder.SelectHttpMethod(config, payload);

            actualHttpMethod.Should().Be(expectedWebhookConfig.HttpMethod);
        }

        public static IEnumerable<object[]> UriDataRouteAndReplace =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                                .WithSource(source => source
                                    .AddReplace("selector", "$.TenantCode")
                                    .AddReplace("orderCode", "$.OrderCode"))
                                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                                .AddRoute(route => route
                                    .WithUri("https://blah.blah.Brand1.eshopworld.com/webhook/{orderCode}")
                                    .WithSelector("Brand1")
                                    .WithNoAuthentication())
                                .AddRoute(route => route
                                    .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                    .WithSelector("*")
                                    .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    "https://blah.blah.tenant1.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source
                                .AddReplace("selector", "$.TenantCode")
                                .AddReplace("orderCode", "$.OrderCode"))
                            .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                .WithSelector("*")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    "https://blah.blah.tenant1.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source
                                .AddReplace("selector", "$.TenantCode")
                                .AddReplace("orderCode", "$.OrderCode"))
                            .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                .WithSelector("*")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"DEV13:00026804\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    "https://blah.blah.tenant1.eshopworld.com/webhook/DEV13%3A00026804"
                }
            };

        public static IEnumerable<object[]> WebhookConfigData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source
                                .AddReplace("selector", "$.TenantCode")
                                .AddReplace("orderCode", "$.OrderCode"))
                            .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.Brand1.eshopworld.com/webhook/{orderCode}")
                                .WithHttpVerb("PUT")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                .WithHttpVerb("GET")
                                .WithSelector("*")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    new WebhookConfigRouteBuilder()
                        .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                        .WithHttpVerb("GET")
                        .WithSelector("*")
                        .WithNoAuthentication()
                        .Create()
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source
                                .AddReplace("selector", "$.TenantCode")
                                .AddReplace("orderCode", "$.OrderCode"))
                            .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.Brand1.eshopworld.com/webhook/{orderCode}")
                                .WithHttpVerb("GET")
                                .WithSelector("tenant0")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                .WithHttpVerb("PUT")
                                .WithSelector("*")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant0\"}",
                    new WebhookConfigRouteBuilder()
                        .WithUri("https://blah.blah.Brand1.eshopworld.com/webhook/{orderCode}")
                        .WithHttpVerb("GET")
                        .WithSelector("tenant0")
                        .WithNoAuthentication()
                        .Create()
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source
                                .AddReplace("selector", "$.TenantCode")
                                .AddReplace("orderCode", "$.OrderCode"))
                            .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                .WithHttpVerb("PUT")
                                .WithSelector("*")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    new WebhookConfigRouteBuilder()
                        .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                        .WithHttpVerb("PUT")
                        .WithSelector("*")
                        .WithNoAuthentication()
                        .Create()
                }
            };
    }
}