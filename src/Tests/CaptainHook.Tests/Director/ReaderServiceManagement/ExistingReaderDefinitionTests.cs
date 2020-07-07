
using CaptainHook.DirectorService.ReaderServiceManagement;
using Eshopworld.Tests.Core;
using Xunit;

namespace CaptainHook.Tests.Director.ReaderServiceManagement
{
    public class ExistingReaderDefinitionTests
    {
        [IsUnit, Theory]
        [InlineData ("some-name", "some-name")]
        [InlineData ("some-name-1234567890abcdeFGHIJxZ", "some-name")]
        [InlineData ("some-name-abcdeFGHIJxZ1234567890", "some-name")]
        [InlineData ("some-name-abcdeFGHIJ1234567890", "some-name")]
        [InlineData ("some-name-1234567890abcdeFGHIJ", "some-name")]
        [InlineData ("some-name-1234567890abcdeFGHIJtoolong", "some-name-1234567890abcdeFGHIJtoolong")]
        [InlineData ("1234567890abcdeFGHIJxZ", "1234567890abcdeFGHIJxZ")]
        [InlineData ("another.000,name-with/some-other:characters", "another.000,name-with/some-other:characters")]
        [InlineData ("service-name-a", "service-name")]
        [InlineData ("service-name-b", "service-name")]
        [InlineData ("service-name-12345678901234", "service-name")]
        [InlineData ("service-name-1234567890123456", "service-name-1234567890123456")]
        [InlineData ("service-name-1234567890", "service-name-1234567890")]
        public void Ctor_WithValidNames_CreatesCorrectInstance (string nameWithSuffix, string expectedServiceName)
        {
            // Arrange
            // Act
            var def = new ExistingReaderDefinition (nameWithSuffix);

            // Assert
            Assert.True (def.IsValid);
            Assert.Equal (nameWithSuffix, def.ServiceNameWithSuffix);
            Assert.Equal (expectedServiceName, def.ServiceName);
        }

        [IsUnit, Fact]
        public void Ctor_Default_CreatesInvalidInstance ()
        {
            // Arrange
            // Act
            var def = new ExistingReaderDefinition();

            // Assert
            Assert.False (def.IsValid);
        }
    }
}
