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

        [Fact, IsUnit]
        public void When_CallbacksIsEmpty_Then_NoFailuresReturned()
        {
            var dto = new SubscriberDtoBuilder().With(x => x.Callbacks, null).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact, IsUnit]
        public void When_DlqIsEmpty_Then_NoFailuresReturned()
        {
            var dto = new SubscriberDtoBuilder().With(x => x.DlqHooks, null).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [ClassData(typeof(InvalidJsonPaths))]
        public void When_WebhooksSelectionRuleIsNotValidJsonPath_Then_ValidationFails(string jsonPath)
        {
            var webhooksDto = new WebhooksDtoBuilder()
                .With(x => x.SelectionRule, jsonPath)
                .With(x => x.Endpoints, new List<EndpointDto>
                {
                    new EndpointDtoBuilder().With(e => e.Selector, "abc").Create()
                })
                .Create();
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooksDto).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks.SelectionRule);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        [InlineData("*")]
        public void When_SingleEndpointWithNullOrDefaultSelectorAndNoSelectionRule_ValidationSucceeds(string selector)
        {
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder().With(x => x.Selector, selector).Create()
            };
            var webhooks = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).With(x => x.SelectionRule, null).Create();
            var subscriber = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooks).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", subscriber);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [InlineData(null)]
        [InlineData("*")]
        public void When_SingleEndpointWithNullOrDefaultSelectorAndSelectionRuleDefined_ValidationSucceeds(string selector)
        {
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder().With(x => x.Selector, selector).Create()
            };
            var webhooks = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).With(x => x.SelectionRule, "$.TenantCode").Create();
            var subscriber = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooks).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", subscriber);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [InlineData(null)]
        [InlineData("*")]
        public void When_ManyEndpointsHaveNullOrDefaultSelector_Then_ValidationFails(string selector)
        {
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder().With(x => x.Selector, selector).Create(),
                new EndpointDtoBuilder().With(x => x.Selector, selector).Create()
            };
            var webhooks = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).Create();
            var subscriber = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooks).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", subscriber);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks.Endpoints)
                .WithErrorMessage("There can be only one endpoint with the default selector");
        }

        [Fact, IsUnit]
        public void When_OneEndpointHaveDefaultSelectorAndAnotherEndpointHaveNullSelector_Then_ValidationFails()
        {
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder().With(x => x.Selector, "*").Create(),
                new EndpointDtoBuilder().With(x => x.Selector, null).Create()
            };
            var webhooks = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).Create();
            var subscriber = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooks).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", subscriber);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks.Endpoints)
                .WithErrorMessage("There can be only one endpoint with the default selector");
        }

        [Fact, IsUnit]
        public void When_ManyEndpointsHaveTheSameCustomSelector_Then_ValidationFails()
        {
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder().With(x => x.Selector, "*").Create(),
                new EndpointDtoBuilder().With(x => x.Selector, "selectorA").Create(),
                new EndpointDtoBuilder().With(x => x.Selector, "selectorA").Create()
            };
            var webhooks = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).Create();
            var subscriber = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooks).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", subscriber);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks.Endpoints)
                .WithErrorMessage("There cannot be multiple endpoints with the same selector");
        }

        [Fact, IsUnit]
        public void When_SingleEndpointWithCustomSelectorAndSelectionRuleIsDefined_Then_ValidationSucceeds()
        {
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder().With(x => x.Selector, "selector1").Create()
            };
            var webhooks = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).With(x => x.SelectionRule, "$.TenantCode").Create();
            var subscriber = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooks).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", subscriber);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact, IsUnit]
        public void When_SingleEndpointWithCustomSelectorAndNoSelectionRule_Then_ValidationFails()
        {
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder().With(x => x.Selector, "selector1").Create()
            };
            var webhooks = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).With(x => x.SelectionRule, null).Create();
            var subscriber = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooks).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", subscriber);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks.SelectionRule);
        }

        [Fact, IsUnit]
        public void When_OneEndpointHasDefaultSelectorAndOtherEndpointsHaveCustomSelectorsAndSelectionRuleIsDefined_Then_ValidationSucceeds()
        {
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder().With(x => x.Selector, "*").Create(),
                new EndpointDtoBuilder().With(x => x.Selector, "selector1").Create(),
                new EndpointDtoBuilder().With(x => x.Selector, "selector2").Create()
            };
            var webhooks = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).With(x => x.SelectionRule, "$.TenantCode").Create();
            var subscriber = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooks).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", subscriber);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact, IsUnit]
        public void When_OneEndpointHasDefaultSelectorAndOtherEndpointsHaveCustomSelectorsAndNoSelectionRuleIsDefined_Then_ValidationFails()
        {
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder().With(x => x.Selector, "*").Create(),
                new EndpointDtoBuilder().With(x => x.Selector, "selector1").Create(),
                new EndpointDtoBuilder().With(x => x.Selector, "selector2").Create()
            };
            var webhooks = new WebhooksDtoBuilder().With(x => x.Endpoints, endpoints).With(x => x.SelectionRule, null).Create();
            var subscriber = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooks).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", subscriber);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks.SelectionRule);
        }

        [Fact, IsUnit]
        public void When_UriTransformDoesNotHaveAllReplacements_Then_ValidationFails()
        {
            var uriTransform = new UriTransformDto
            {
                Replace = new Dictionary<string, string>
                {
                    { "token1", "Value1" }
                }
            };
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder()
                    .With(x => x.Selector, "*")
                    .With(x => x.Uri, "http://www.uri1.com/{selector}/{token1}")
                    .Create(),
                new EndpointDtoBuilder()
                    .With(x => x.Selector, "selector1")
                    .With(x => x.Uri, "http://www.uri2.com/{selector}/{token2}")
                    .Create()
            };
            var webhooksDto = new WebhooksDtoBuilder()
                .With(x => x.Endpoints, endpoints)
                .With(x => x.UriTransform, uriTransform)
                .Create();
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooksDto).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Subscriber.Webhooks.UriTransform.Replace);
        }

        [Fact, IsUnit]
        public void When_UriTransformHasAllReplacements_Then_ValidationSucceeds()
        {
            var uriTransform = new UriTransformDto
            {
                Replace = new Dictionary<string, string>
                {
                    { "token1", "Value1" },
                    { "token2", "Value2" },
                    { "token3", "Value3" }
                }
            };
            var endpoints = new List<EndpointDto>
            {
                new EndpointDtoBuilder()
                    .With(x => x.Selector, "*")
                    .With(x => x.Uri, "http://www.uri1.com/{selector}/{token1}")
                    .Create(),
                new EndpointDtoBuilder()
                    .With(x => x.Selector, "selector1")
                    .With(x => x.Uri, "http://www.uri2.com/{selector}/{token2}")
                    .Create()
            };
            var webhooksDto = new WebhooksDtoBuilder()
                .With(x => x.Endpoints, endpoints)
                .With(x => x.UriTransform, uriTransform)
                .Create();
            var dto = new SubscriberDtoBuilder().With(x => x.Webhooks, webhooksDto).Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
