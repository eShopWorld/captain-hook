using CaptainHook.Application.Validators.Dtos;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Application.Tests.Validators.Dtos
{
    public class UpsertSubscriberEndpointDtoValidatorTests
    {
        private readonly UpsertSubscriberEndpointDtoValidator _defaultValidator = new UpsertSubscriberEndpointDtoValidator();

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void Validate_NoSelectorProvided_FailedValidation(string selectorString)
        {
            // Arrange
            var endpoint = new EndpointDto { Selector = selectorString };

            // Act
            var result = _defaultValidator.TestValidate(endpoint);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Selector);
        }

        [Theory, IsUnit]
        [InlineData("abc")]
        [InlineData("*")]
        public void Validate_SelectorProvided_SuccessValidation(string selectorString)
        {
            // Arrange
            var endpoint = new EndpointDto { Selector = selectorString };

            // Act
            var result = _defaultValidator.TestValidate(endpoint);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Selector);
        }
    }
}