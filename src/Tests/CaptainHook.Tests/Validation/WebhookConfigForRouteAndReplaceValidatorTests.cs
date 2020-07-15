using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Validation;
using CaptainHook.Tests.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Tests.Validation
{
    public class WebhookConfigForRouteAndReplaceValidatorTests
    {
        private readonly WebhookConfigForRouteAndReplaceValidator _validator = new WebhookConfigForRouteAndReplaceValidator();

        private static WebhookConfigBuilder GetValidWebhookConfigBuilder()
        {
            var webhookConfigBuilder = new WebhookConfigBuilder()
                .SetWebhookRequestRule(ruleBuilder => ruleBuilder
                    .WithSource(sourceBuilder => sourceBuilder
                        .WithLocation(Location.Body).WithRuleAction(RuleAction.RouteAndReplace)
                        .AddReplace("selector", "something")
                        .AddReplace("action", "something-else")
                        .AddReplace("orderCode", "other-value")
                    )
                    .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                    .AddRoute(routeBuilder => routeBuilder.WithUri("https://api-{selector}.company.com/api/v2/{action}/Put/{orderCode}"))
                );

            return webhookConfigBuilder;
        }

        [Fact, IsUnit]
        public void When_WebhookConfig_is_valid_for_RouteAndReplace_then_no_failures_should_be_returned()
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeTrue();
        }

        [Theory, IsUnit]
        [InlineData(RuleAction.Add)]
        [InlineData(RuleAction.Replace)]
        [InlineData(RuleAction.Route)]
        public void When_no_Rule_has_Destination_RuleAction_set_to_RouteAndReplace_then_validation_should_fail(RuleAction ruleAction)
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();
            webhookConfig.WebhookRequestRules[0].Destination.RuleAction = ruleAction;

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
        }

        [Fact, IsUnit]
        public void When_Replace_does_not_contain_an_item_with_key_selector_then_validation_should_fail()
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();
            webhookConfig.WebhookRequestRules[0].Source.Replace = new Dictionary<string, string>
            {
                ["not-selector"] = "something"
            };

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
        }

        [Theory, IsUnit]
        [InlineData(Location.Uri)]
        [InlineData(Location.Header)]
        [InlineData(Location.HttpContent)]
        [InlineData(Location.HttpStatusCode)]
        public void When_Source_Location_is_not_Body_then_validation_should_fail(Location location)
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();
            webhookConfig.WebhookRequestRules[0].Source.Location = location;

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
        }

        [Theory, IsUnit]
        [InlineData("selector", null)]
        [InlineData("selector", "")]
        [InlineData("non-selector", null)]
        [InlineData("non-selector", "")]
        public void When_Replace_contains_empty_values_then_validation_should_fail(string key, string invalidValue)
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();
            webhookConfig.WebhookRequestRules[0].Source.Replace = new Dictionary<string, string>
            {
                [key] = invalidValue
            };

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
        }

        [Fact(Skip = "verify if needed"), IsUnit]
        public void When_Replace_does_not_contain_key_which_exist_in_Route_Uri_then_validation_should_fail()
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();
            webhookConfig.WebhookRequestRules[0].Routes[0].Uri = "https://test-api-{selector}.company.com/api/v2/{action-WebHook}/Put/{orderCode}";
            webhookConfig.WebhookRequestRules[0].Source.Replace = new Dictionary<string, string>
            {
                ["selector"] = "something",
                ["action"] = "other-action"
            };

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
        }

        [Fact, IsUnit]
        public void When_Routes_does_not_contain_any_Uri_then_validation_should_fail()
        {

        }

        [Fact, IsUnit]
        public void When_more_than_one_Route_Uri_is_default_then_validation_should_fail()
        {

        }

        [Fact, IsUnit]
        public void When_Route_Uri_selector_is_empty_then_validation_should_fail()
        {

        }

        [Fact, IsUnit]
        public void When_Route_Uri_selector_is_not_unique_then_validation_should_fail()
        {

        }
    }
}
