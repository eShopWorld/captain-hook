using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.FeatureFlags;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class FeatureFlagsConfigurationTests
    {
        [Fact, IsUnit]
        public void GetFlag_NoFlagsConfigured_IsDisabled()
        {
            // Arrange
            var featureFlagsConfiguration = new FeatureFlagsConfiguration();

            // Act
            var actual = featureFlagsConfiguration.GetFlag<TestFeatureFlag>();

            // Assert
            var expected = new TestFeatureFlag();
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public void GetFlag_UnknownFlagConfigured_TestFeatureFlagIsDisabled()
        {
            // Arrange
            var featureFlagsConfiguration = new FeatureFlagsConfiguration
            {
                FeatureFlags = { "dummy-ff" }
            };

            // Act
            var actual = featureFlagsConfiguration.GetFlag<TestFeatureFlag>();

            // Assert
            var expected = new TestFeatureFlag();
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public void GetFlag_TestFeatureFlagConfigured_IsEnabled()
        {
            // Arrange
            var featureFlagsConfiguration = new FeatureFlagsConfiguration
            {
                FeatureFlags = { "test-ff" }
            };

            // Act
            var actual = featureFlagsConfiguration.GetFlag<TestFeatureFlag>();

            // Assert
            using (new AssertionScope())
            {
                actual.Identifier.Should().Be("test-ff");
                actual.IsEnabled.Should().BeTrue("because that feature flag was enabled");
            }
        }

        private class TestFeatureFlag: FeatureFlagBase
        {
            public TestFeatureFlag(): base("test-ff")
            {
            }
        }
    }
}