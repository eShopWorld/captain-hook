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
        private readonly WebhooksEntityValidator _validator = new WebhooksEntityValidator();

        [Fact, IsUnit]
        public void Validate_EmptyCollectionValidationPassedOnToAnotherValidator_EmptyListOfEndpointsNotAllowed()
        {
            // Arrange
            var endpoints = new WebhooksEntity(null, null);

            // Act
            var result = _validator.TestValidate(endpoints);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Endpoints)
                .WithErrorMessage("Webhooks list must contain at list one endpoint");
        }

        [Fact, IsUnit]
        public void Validate_NoSelectionRuleAndCollectionWithSpecificSelector_FailsValidation()
        {
            // Arrange
            var endpoints = new WebhooksEntity(null, new []
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

        [Fact, IsUnit]
        public void Validate_NoSelectionRuleAndCollectionWithSpecificSelectorAndNoSelector_FailsValidation()
        {
            // Arrange
            var endpoints = new WebhooksEntity(null, new[]
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
            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("Only a single default endpoint is allowed if no selection rule provided");
        }

        [Fact, IsUnit]
        public void Validate_NoSelectionRuleAndCollectionWithMultipleDefaultEndpoints_FailsValidation()
        {
            // Arrange
            var endpoints = new WebhooksEntity(null, new[]
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
            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("Only a single default endpoint is allowed if no selection rule provided");
        }

        [Fact, IsUnit]
        public void Validate_SelectionRuleInPlaceAndCollectionWithSpecificSelectorAndNoSelector_SucceedsValidation()
        {
            // Arrange
            var endpoints = new WebhooksEntity("$.Test", new[]
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

        [Fact, IsUnit]
        public void Validate_SelectionRuleInPlaceAndCollectionWithSpecificSelector_SucceedsValidation()
        {
            // Arrange
            var endpoints = new WebhooksEntity("$.Test", new[]
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

        [Fact, IsUnit]
        public void Validate_WebhookWithTokenNotInUriTransform_FailsValidation()
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string>()
            {
                { "token1", "value1" },
                { "token2", "value2" }
            });
            var endpoints = new WebhooksEntity("$.Test", new[]
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

        [Fact, IsUnit]
        public void Validate_WebhookWithAllTokensInUriTransform_SucceedsValidation()
        {
            // Arrange
            var uriTransform = new UriTransformEntity(new Dictionary<string, string>()
            {
                { "token1", "value1" },
                { "token2", "value2" }
            });
            var endpoints = new WebhooksEntity("$.Test", new[]
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