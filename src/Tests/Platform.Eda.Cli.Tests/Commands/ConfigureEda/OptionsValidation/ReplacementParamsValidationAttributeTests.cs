using System.ComponentModel.DataAnnotations;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Platform.Eda.Cli.Commands.ConfigureEda.OptionsValidation;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.OptionsValidation
{
    public class ReplacementParamsValidationAttributeTests
    {
        [Theory, IsUnit]
        [InlineData("abc=def")]
        [InlineData("my-domain=\"test.domain.com\"")]
        public void ForValidParam_ReturnsTrue(string validValue)
        {
            var result = new ReplacementParamsValidationAttribute().GetValidationResult(validValue, new ValidationContext(new object()));

            result.Should().Be(ValidationResult.Success);
        }

        [Theory, IsUnit]
        [InlineData("abc=")]
        [InlineData("=def")]
        [InlineData("abc")]
        [InlineData("abc =def")]
        public void ForInvalidParam_ReturnsError(string invalidValue)
        {
            var result = new ReplacementParamsValidationAttribute().GetValidationResult(invalidValue, new ValidationContext(new object()));

            result.Should().NotBe(ValidationResult.Success);
        }
    }
}