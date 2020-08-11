using System.Collections.Generic;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure.Builders;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
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
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_EventIsEmpty_Then_ValidationFails(string invalidString)
        {
            // Arrange
            var dto = new SubscriberDtoBuilder().Create();
            var request = new UpsertSubscriberRequest(invalidString, "subscriber", dto);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EventName);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_SubscriberIsEmpty_Then_ValidationFails(string invalidString)
        {
            // Arrange
            var dto = new SubscriberDtoBuilder().Create();
            var request = new UpsertSubscriberRequest("event", invalidString, dto);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SubscriberName);
        }

        [Fact, IsUnit]
        public void When_WebhooksIsNull_Then_ValidationFails()
        {
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, null).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks);
        }

        [Fact, IsUnit]
        public void When_WebhooksIsEmpty_Then_ValidationFails()
        {
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, null).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks);
        }

        [Theory, IsUnit]
        [ClassData(typeof(InvalidJsonPaths))]
        public void When_WebhooksSelectionRuleIsNotValidJsonPath_Then_ValidationFails(string jsonPath)
        {
            var webhooksDto = new WebhooksDtoBuilder().With(x => x.SelectionRule, jsonPath).Create();
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooksDto).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks.SelectionRule);
        }

        [Fact, IsUnit]
        public void When_TwoEndpointsHaveNoSelector_Then_ValidationFails()
        {
            var endpoints = new List<EndpointDto>()
            {
                new EndpointDtoBuilder().With(x => x.Selector, null).Create(),
                new EndpointDtoBuilder().With(x => x.Selector, null).Create()
            };
            var webhooksDto = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).Create();
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooksDto).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks.Endpoints);
        }

        [Fact, IsUnit]
        public void When_TwoEndpointsHaveTheSameSelector_Then_ValidationFails()
        {
            var endpoints = new List<EndpointDto>()
            {
                new EndpointDtoBuilder().With(x => x.Selector, null).Create(),
                new EndpointDtoBuilder().With(x => x.Selector, "selectorA").Create(),
                new EndpointDtoBuilder().With(x => x.Selector, "selectorA").Create()
            };
            var webhooksDto = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).Create();
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooksDto).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks.Endpoints);
        }

        [Fact, IsUnit]
        public void When_OnlyOneEndpointHasNoSelector_Then_ValidationSucceeds()
        {
            var endpoints = new List<EndpointDto>()
            {
                new EndpointDtoBuilder().With(x => x.Selector, null).Create(),
                new EndpointDtoBuilder().With(x => x.Selector, "selector1").Create(),
                new EndpointDtoBuilder().With(x => x.Selector, "selector2").Create()
            };
            var webhooksDto = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).Create();
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooksDto).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
