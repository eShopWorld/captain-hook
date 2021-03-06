﻿using System;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.ServiceFabric.Services.Remoting;
using Xunit;

namespace CaptainHook.Telemetry.Tests
{
    public class FabricTransportServiceRemotingProviderWithTelemetryAttributeTests
    {
        [Fact, IsUnit]
        public void FabricTransportServiceRemotingProviderWithTelemetryAttributeCreatesRemotingListener()
        {
            var attr = new FabricTransportServiceRemotingProviderWithTelemetryAttribute();

            var dict = attr.CreateServiceRemotingListeners();

            dict.Should().NotBeEmpty();
        }

        [Fact, IsUnit]
        public void FabricTransportServiceRemotingProviderWithTelemetryAttributeDoesNotAcceptV2_1Listener()
        {
            var attr = new FabricTransportServiceRemotingProviderWithTelemetryAttribute
            {
                RemotingListenerVersion = RemotingListenerVersion.V2_1,
            };

            Action func = () => attr.CreateServiceRemotingListeners();

            func.Should().Throw<InvalidOperationException>();
        }
    }
}