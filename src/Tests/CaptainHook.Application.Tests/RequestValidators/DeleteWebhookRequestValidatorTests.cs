using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Application.Tests.RequestValidators
{
    public class DeleteWebhookRequestValidatorTests
    {
        private readonly DeleteWebhookRequestValidator _validator = new DeleteWebhookRequestValidator();

        [Theory, IsUnit]
        [InlineData("selector")]
        [InlineData(null)]
        public void When_RequestIsValid_Then_NoFailuresReturned(string validSelector)
        {
            var request = new DeleteWebhookRequest("event", "subscriber", validSelector);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [InlineData("")]
        [InlineData("   ")]
        public void When_SelectorIsINvalid_Then_ValidationFails(string selector)
        {
            var request = new DeleteWebhookRequest("event", "subscriber", selector);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Selector);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_EventIsEmpty_Then_ValidationFails(string invalidString)
        {
            var request = new DeleteWebhookRequest(invalidString, "subscriber", "selector");

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.EventName);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_SubscriberIsEmpty_Then_ValidationFails(string invalidString)
        {
            var request = new DeleteWebhookRequest("event", invalidString, "selector");

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.SubscriberName);
        }
    }
}