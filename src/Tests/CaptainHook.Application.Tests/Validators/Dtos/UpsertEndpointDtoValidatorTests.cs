using CaptainHook.Application.Validators.Dtos;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Application.Tests.Validators.Dtos
{
    public class UpsertEndpointDtoValidatorTests
    {
        private readonly UpsertEndpointDtoValidator _defaultValidator = new UpsertEndpointDtoValidator("*");

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void Validate_NoSelectorProvided_SuccessValidation(string selectorString)
        {
            // Arrange
            var endpoint = new EndpointDto { Selector = selectorString };

            // Act
            var result = _defaultValidator.TestValidate(endpoint);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Selector);
        }

        [Fact, IsUnit]
        public void Validate_DifferentSelectorProvided_FailedValidation()
        {
            // Arrange
            var endpoint = new EndpointDto { Selector = "abc" };

            // Act
            var result = _defaultValidator.TestValidate(endpoint);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Selector)
                .WithErrorMessage("Selector has to match the selector identifier or be empty");
        }

        [Theory, IsUnit]
        [InlineData("*")]
        [InlineData("abc")]
        public void Validate_SameSelectorProvided_SuccessValidation(string selector)
        {
            // Arrange
            var validator = new UpsertEndpointDtoValidator(selector);
            var endpoint = new EndpointDto { Selector = selector };

            // Act
            var result = validator.TestValidate(endpoint);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Selector);
        }
    }
}