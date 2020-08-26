﻿using CaptainHook.Common.Configuration.FeatureFlags;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace CaptainHook.Tests.Configuration.FeatureFlags
{
    public class DisablePayloadLoggingFeatureFlagTests
    {
        [Fact, IsUnit]
        public void CreatesDefaultInstance()
        {
            // Arrange
            var featureFlag = new DisablePayloadLoggingFeatureFlag();

            // Assert
            using (new AssertionScope())
            {
                featureFlag.Identifier.Should().Be("DisablePayloadLogging");
                featureFlag.IsEnabled.Should().BeFalse("because by default features are off");
            }
        }

        [Fact, IsUnit]
        public void CanChangeEnabledProperty()
        {
            // Arrange
            var featureFlag = new DisablePayloadLoggingFeatureFlag();
            featureFlag.SetEnabled(true);

            // Assert
            using (new AssertionScope())
            {
                featureFlag.Identifier.Should().Be("DisablePayloadLogging");
                featureFlag.IsEnabled.Should().BeTrue("because the default value has been overriden");
            }
        }
    }
}