using CaptainHook.Domain.Entities;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
using System.Collections.Generic;
using Xunit;

namespace CaptainHook.Domain.Tests.Entities
{
    public class WebhooksEntityValidatorTests
    {
        private static readonly WebhooksEntityValidator _validator = new WebhooksEntityValidator();

        [Fact, IsUnit]
        public void Validate_EmptyWebooksCollectionValidationPassedOnToAnotherValidator_EmptyListOfEndpointsNotAllowed()
        {
            // Arrange
            var endpoints = new WebhooksEntity(WebhooksEntityType.Webhooks);

            // Act
            var result = _validator.TestValidate(endpoints);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Endpoints)
                .WithErrorMessage("Webhooks list must contain at list one endpoint");
        }

        [Fact, IsUnit]
        public void Validate_EmptyCallbacksCollectionValidationPassedOnToAnotherValidator_EmptyListOfEndpointsAllowed()
        {
            // Arrange
            var endpoints = new WebhooksEntity(WebhooksEntityType.Callbacks);

            // Act
            var result = _validator.TestValidate(endpoints);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }


        [Theory, IsUnit]
        [InlineData(WebhooksEntityType.Webhooks)]
        [InlineData(WebhooksEntityType.Callbacks)]
        public void Validate_NoSelectionRuleAndWebooksCollectionWithSpecificSelector_FailsValidation(WebhooksEntityType type)
        {
            // Arrange
            var endpoints = new WebhooksEntity(type, null, new []
            {
                new EndpointBuilder()
                    .WithSelector("abc")
                    .WithUri("https://uri.com")
                    .Create()
            });

            // Act
            var result = _validator.TestValidate(endpoints);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("Only a single default endpoint is allowed if no selection rule provided");
        }

        [Theory, IsUnit]
        [InlineData(WebhooksEntityType.Webhooks)]
        [InlineData(WebhooksEntityType.Callbacks)]
        public void Validate_NoSelectionRuleAndWebooksCollectionWithSpecificSelectorAndNoSelector_FailsValidation(WebhooksEntityType type)
        {
            // Arrange
            var endpoints = new WebhooksEntity(type, null, new[]
            {
                new EndpointBuilder()
                    .WithSelector("abc")
                    .WithUri("https://uri.com")
                    .Create(),
                new EndpointBuilder()
                    .WithSelector("*")
                    .WithUri("https://uri2.com")
                    .Create()
            });

            // Act
            var result = _validator.TestValidate(endpoints);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("Only a single default endpoint is allowed if no selection rule provided");
        }

        [Theory, IsUnit]
        [InlineData(WebhooksEntityType.Webhooks)]
        [InlineData(WebhooksEntityType.Callbacks)]
        public void Validate_NoSelectionRuleAndWebooksCollectionWithMultipleDefaultEndpoints_FailsValidation(WebhooksEntityType type)
        {
            // Arrange
            var endpoints = new WebhooksEntity(type, null, new[]
            {
                new EndpointBuilder()
                    .WithSelector(null)
                    .WithUri("https://uri.com")
                    .Create(),
                new EndpointBuilder()
                    .WithSelector(null)
                    .WithUri("https://uri2.com")
                    .Create()
            });

            // Act
            var result = _validator.TestValidate(endpoints);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Endpoints);
        }

        [Theory, IsUnit]
        [InlineData(WebhooksEntityType.Webhooks)]
        [InlineData(WebhooksEntityType.Callbacks)]
        public void Validate_SelectionRuleInPlaceAndWebooksCollectionWithSpecificSelectorAndNoSelector_SucceedsValidation(WebhooksEntityType type)
        {
            // Arrange
            var endpoints = new WebhooksEntity(type, "$.Test", new[]
            {
                new EndpointBuilder()
                    .WithSelector("abc")
                    .WithUri("https://uri.com")
                    .Create(),
                new EndpointBuilder()
                    .WithSelector(null)
                    .WithUri("https://uri2.com")
                    .Create()
            });

            // Act
            var result = _validator.TestValidate(endpoints);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [InlineData(WebhooksEntityType.Webhooks)]
        [InlineData(WebhooksEntityType.Callbacks)]
        public void Validate_SelectionRuleInPlaceAndWebooksCollectionWithSpecificSelector_SucceedsValidation(WebhooksEntityType type)
        {
            // Arrange
            var endpoints = new WebhooksEntity(type, "$.Test", new[]
            {
                new EndpointBuilder()
                    .WithSelector("abc")
                    .WithUri("https://uri.com")
                    .Create()
            });

            // Act
            var result = _validator.TestValidate(endpoints);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [InlineData(WebhooksEntityType.Webhooks)]
        [InlineData(WebhooksEntityType.Callbacks)]
        public void Validate_WebhookWithTokenNotInUriTransform_FailsValidation(WebhooksEntityType type)
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string>()
            {
                { "token1", "value1" },
                { "token2", "value2" }
            });
            var endpoints = new WebhooksEntity(type, "$.Test", new[]
            {
                new EndpointBuilder()
                    .WithSelector("abc")
                    .WithUri("https://uri.com/{selector}/{token3}")
                    .Create()
            }, uriTransform);

            // Act
            var result = _validator.TestValidate(endpoints);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UriTransform.Replace);
        }

        [Theory, IsUnit]
        [InlineData(WebhooksEntityType.Webhooks)]
        [InlineData(WebhooksEntityType.Callbacks)]
        public void Validate_WebhookWithAllTokensInUriTransform_SucceedsValidation(WebhooksEntityType type)
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string>()
            {
                { "token1", "value1" },
                { "token2", "value2" }
            });
            var endpoints = new WebhooksEntity(type, "$.Test", new[]
            {
                new EndpointBuilder()
                    .WithSelector("abc1")
                    .WithUri("https://uri1.com/{selector}/{token1}")
                    .Create(),
                new EndpointBuilder()
                    .WithSelector("abc2")
                    .WithUri("https://uri2.com/{selector}/{token2}")
                    .Create()
            }, uriTransform);

            // Act
            var result = _validator.TestValidate(endpoints);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
   }
}