using CaptainHook.Common.Configuration.FeatureFlags;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace CaptainHook.Tests.Configuration.FeatureFlags
{
    public class CosmosDbFeatureFlagTests
    {
        [Fact, IsUnit]
        public void CreatesDefaultInstance()
        {
            // Arrange
            var cosmosDbFeatureFlag = new CosmosDbFeatureFlag();

            // Assert
            using (new AssertionScope())
            {
                cosmosDbFeatureFlag.Identifier.Should().Be("CosmosDb");
                cosmosDbFeatureFlag.IsEnabled.Should().BeFalse("because by default features are off");
            }
        }

        [Fact, IsUnit]
        public void CanChangeEnabledProperty()
        {
            // Arrange
            var cosmosDbFeatureFlag = new CosmosDbFeatureFlag();
            cosmosDbFeatureFlag.SetEnabled(true);

            // Assert
            using (new AssertionScope())
            {
                cosmosDbFeatureFlag.Identifier.Should().Be("CosmosDb");
                cosmosDbFeatureFlag.IsEnabled.Should().BeTrue("because the default value has been overriden");
            }
        }
    }
}