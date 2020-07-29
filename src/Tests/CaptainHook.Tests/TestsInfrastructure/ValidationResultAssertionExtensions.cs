using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.Results;

namespace CaptainHook.Tests.TestsInfrastructure
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
    }
}