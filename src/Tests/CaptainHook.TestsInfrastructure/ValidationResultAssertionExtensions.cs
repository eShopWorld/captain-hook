using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.Results;

namespace CaptainHook.TestsInfrastructure
{
    public static class ValidationResultAssertionExtensions
    {
        public static void AssertSingleFailure(this ValidationResult result, string propertyName)
        {
            using var assertionScope = new AssertionScope();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().EndWith(propertyName);
        }

        public static void AssertValidationSuccess(this ValidationResult result)
        {
            result.IsValid.Should().BeTrue();
        }
    }
}