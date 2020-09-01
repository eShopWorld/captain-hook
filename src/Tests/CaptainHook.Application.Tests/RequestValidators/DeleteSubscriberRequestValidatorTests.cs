using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Application.Tests.RequestValidators
{
    public class DeleteSubscriberRequestValidatorTests
    {
        private readonly DeleteSubscriberRequestValidator _validator = new DeleteSubscriberRequestValidator();

        [Fact, IsUnit]
        public void When_RequestIsValid_Then_NoFailuresReturned()
        {
            var request = new DeleteSubscriberRequest("event", "subscriber");

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_EventIsEmpty_Then_ValidationFails(string invalidString)
        {
            var request = new DeleteSubscriberRequest(invalidString, "subscriber");

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.EventName);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_SubscriberIsEmpty_Then_ValidationFails(string invalidString)
        {
            var request = new DeleteSubscriberRequest("event", invalidString);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.SubscriberName);
        }
    }
}