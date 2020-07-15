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

        [Fact, IsUnit]
        public void When_WebhookConfig_is_valid_for_RouteAndReplace_then_no_failures_should_be_returned()
        {
            var webhookConfig = new WebhookConfigBuilder().Create();

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeTrue();
        }

        [Fact, IsUnit]
        public void When_no_Rule_has_Destination_RuleAction_set_to_RouteAndReplace_then_validation_should_fail()
        {
            var webhookConfig = new WebhookConfigBuilder().Create();

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeFalse();
        }

        [Fact, IsUnit]
        public void When_Replace_does_not_contain_an_item_with_key_selector_then_validation_should_fail()
        {

        }

        [Fact, IsUnit]
        public void When_Replace_contains_an_item_with_key_selector_but_with_empty_value_then_validation_should_fail()
        {

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
