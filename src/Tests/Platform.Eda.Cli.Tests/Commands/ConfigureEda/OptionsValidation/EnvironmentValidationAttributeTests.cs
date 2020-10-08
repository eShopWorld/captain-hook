using System.ComponentModel.DataAnnotations;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Platform.Eda.Cli.Commands.ConfigureEda.OptionsValidation;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.OptionsValidation
{
    public class EnvironmentValidationAttributeTests
    {
        private readonly EnvironmentValidationAttribute _validationAttribute = new EnvironmentValidationAttribute();

        [Theory, IsUnit]
        [InlineData("abc")]
        [InlineData("ci")]
        [InlineData("prep")]
        [InlineData("PRODUCTION")]
        [InlineData("_")]
        public void GetValidationResult_InvalidEnvironmentProvided_FailsValidationWithMessage(string environment)
        {
            // Act
            var result = _validationAttribute.GetValidationResult(environment, new ValidationContext(new object()));

            // Assert
            var expectedResult = new ValidationResult($"Wrong environment {environment}. Choose one from: CI, TEST, PREP, SAND, PROD");
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Theory, IsUnit]
        [InlineData("CI")]
        [InlineData("TEST")]
        [InlineData("PREP")]
        [InlineData("SAND")]
        [InlineData("PROD")]
        [InlineData("")] // empty is valid as for empty environment we are going to use CI automatically
        public void GetValidationResult_ValidEnvironmentProvided_SuccessValidation(string environment)
        {
            // Act
            var result = _validationAttribute.GetValidationResult(environment, new ValidationContext(new object()));

            // Assert
            result.Should().Be(ValidationResult.Success);
        }
    }
}