using System.ComponentModel.DataAnnotations;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Platform.Eda.Cli.Commands.ConfigureEda.OptionsValidation;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.OptionsValidation
{
    public class ReplacementParamsDictionaryAttributeTests
    {
        [Fact, IsUnit]
        public void ForValidParam_ReturnsTrue()
        {
            var result = new ReplacementParamsDictionaryAttribute().GetValidationResult("abc=def", new ValidationContext(new object()));

            result.Should().Be(ValidationResult.Success);
        }

        [Theory, IsUnit]
        [InlineData("abc=")]
        [InlineData("=def")]
        [InlineData("abc")]
        public void ForInvalidParam_ReturnsError(string invalidValue)
        {
            var result = new ReplacementParamsDictionaryAttribute().GetValidationResult(invalidValue, new ValidationContext(new object()));

            result.Should().NotBe(ValidationResult.Success);
        }
    }
}