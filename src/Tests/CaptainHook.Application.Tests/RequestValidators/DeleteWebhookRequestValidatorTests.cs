using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Application.Tests.RequestValidators
{
    public class DeleteWebhookRequestValidatorTests
    {
        private readonly DeleteWebhookRequestValidator _validator = new DeleteWebhookRequestValidator();

        [Fact, IsUnit]
        public void When_RequestIsValid_Then_NoFailuresReturned()
        {
            var request = new DeleteWebhookRequest("event", "subscriber", "selector");

            var result = _validator.TestValidate(request);

            AssertionExtensions.Should(result.IsValid).BeTrue();
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

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_SelectorIsEmpty_Then_ValidationFails(string invalidString)
        {
            var request = new DeleteWebhookRequest("event", "subscriber", invalidString);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Selector);
        }
    }
}