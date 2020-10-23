using System.Collections.Generic;
using CaptainHook.Application.Validators.Dtos;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Application.Tests.Validators.Dtos
{
    public class WebhooksDtoValidatorTests
    {
        private readonly WebhooksDtoValidator _webhooksValidator = new WebhooksDtoValidator(WebhooksValidatorDtoType.Webhook);

        [Fact, IsUnit]
        public void Validate_EndpointsWithSelectorPresent_ErrorOnSelectionRuleMissing()
        {
            // Arrange
            var webhooksDto = new WebhooksDtoBuilder()
                .With(x => x.SelectionRule, null)
                .With(x => x.Endpoints, new List<EndpointDto>
                {
                    new EndpointDtoBuilder()
                        .With(x => x.Selector, "abc")
                        .Create()
                })
                .Create();

            // Act
            var result = _webhooksValidator.TestValidate(webhooksDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SelectionRule)
                .WithErrorMessage("'Selection Rule' must not be empty.");
        }

        [Fact, IsUnit]
        public void Validate_EndpointsWithSelectorPresent_NoErrorOnSelectionRuleExpected()
        {
            // Arrange
            var webhooksDto = new WebhooksDtoBuilder()
                .With(x => x.SelectionRule, "$.Test")
                .With(x => x.Endpoints, new List<EndpointDto>
                {
                    new EndpointDtoBuilder()
                        .With(x => x.Selector, "abc")
                        .Create()
                })
                .Create();

            // Act
            var result = _webhooksValidator.TestValidate(webhooksDto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SelectionRule);
        }

        [Fact, IsUnit]
        public void Validate_UriTransformPresent_ErrorOnSelectionRuleMissing()
        {
            // Arrange
            var webhooksDto = new WebhooksDtoBuilder()
                .With(x => x.SelectionRule, null)
                .With(x => x.UriTransform, new UriTransformDto())
                .Create();

            // Act
            var result = _webhooksValidator.TestValidate(webhooksDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SelectionRule)
                .WithErrorMessage("'Selection Rule' must not be empty.");
        }

        [Fact, IsUnit]
        public void Validate_UriTransformPresent_NoErrorOnSelectionRuleExpected()
        {
            // Arrange
            var webhooksDto = new WebhooksDtoBuilder()
                .With(x => x.SelectionRule, "$.Test")
                .With(x => x.UriTransform, new UriTransformDto())
                .Create();

            // Act
            var result = _webhooksValidator.TestValidate(webhooksDto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SelectionRule);
        }
    }
}