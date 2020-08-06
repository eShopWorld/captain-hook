using System.Collections.Generic;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure;
using CaptainHook.TestsInfrastructure.Builders;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Application.Tests.RequestValidators
{
    public class UpsertSubscriberRequestValidatorTests
    {
        private readonly UpsertSubscriberRequestValidator _validator = new UpsertSubscriberRequestValidator();

        [Fact, IsUnit]
        public void When_RequestIsValid_Then_NoFailuresReturned()
        {
            // Arrange
            var dto = new SubscriberDtoBuilder().Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.AssertValidationSuccess();
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_EventIsEmpty_Then_ValidationFails(string invalidString)
        {
            // Arrange
            var dto = new SubscriberDtoBuilder().Create();
            var request = new UpsertSubscriberRequest(invalidString, "subscriber", dto);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.AssertSingleFailure(nameof(UpsertSubscriberRequest.EventName));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_SubscriberIsEmpty_Then_ValidationFails(string invalidString)
        {
            // Arrange
            var dto = new SubscriberDtoBuilder().Create();
            var request = new UpsertSubscriberRequest("event", invalidString, dto);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.AssertSingleFailure(nameof(UpsertSubscriberRequest.SubscriberName));
        }

        [Fact, IsUnit]
        public void When_WebhooksIsNull_Then_ValidationFails()
        {
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, null).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(SubscriberDto.Webhooks));
        }

        [Fact, IsUnit]
        public void When_WebhooksIsEmpty_Then_ValidationFails()
        {
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, null).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(SubscriberDto.Webhooks));
        }

        [Theory, IsUnit]
        [ClassData(typeof(InvalidJsonPaths))]
        public void When_WebhooksSelectionRuleIsNotValidJsonPath_Then_ValidationFails(string jsonPath)
        {
            var webhooksDto = new WebhooksDtoBuilder().With(x => x.SelectionRule, jsonPath).Create();
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooksDto).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(WebhooksDto.SelectionRule));
        }
    }
}
