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
                .SetWebhookRequestRule(rb => rb
                    .WithSource(sb => sb
                        .WithLocation(Location.Body).WithRuleAction(RuleAction.RouteAndReplace)
                        .AddReplace("selector", "something"))
                    .WithDestination(ruleAction: RuleAction.RouteAndReplace)
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
                //.SetWebhookRequestRule(rb => rb.WithDestination(ruleAction: ruleAction))
                //.Create();

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
                
               // new WebhookConfigBuilder()
               // .AddWebhookRequestRule(rb => rb
               //    .WithSource(sb => sb.WithLocation(Location.Body).WithRuleAction(RuleAction.RouteAndReplace).AddReplace("not-selector", "something"))
               //    .WithDestination(ruleAction: RuleAction.RouteAndReplace))
               //.Create();

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);

            //result.Errors.Should().ContainSingle(failure => failure.PropertyName == nameof(SourceParserLocation.Replace));
        }

        [Theory, IsUnit]
        [InlineData(null)]
        [InlineData("")]
        public void When_Replace_contains_an_item_with_key_selector_but_with_empty_value_then_validation_should_fail(string invalidValue)
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();
            webhookConfig.WebhookRequestRules[0].Source.Replace = new Dictionary<string, string>
            {
                ["selector"] = invalidValue
            };

            //var webhookConfig = new WebhookConfigBuilder()
            //  .AddWebhookRequestRule(rb => rb
            //     .WithSource(sb => sb.WithLocation(Location.Body).WithRuleAction(RuleAction.RouteAndReplace).AddReplace("selector", string.Empty))
            //     .WithDestination(ruleAction: RuleAction.RouteAndReplace))
            // .Create();

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);

            //result.Errors.Should().ContainSingle(failure => failure.PropertyName == nameof(SourceParserLocation.Replace));
        }

        [Fact, IsUnit]
        public void When_Source_Location_is_not_Body_then_validation_should_fail()
        {

        }

        [Fact, IsUnit]
        public void When_Replace_contains_empty_values_then_validation_should_fail()
        {

        }

        [Fact, IsUnit]
        public void When_Replace_does_not_contain_key_which_exist_in_Route_Uri_then_validation_should_fail()
        {
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
