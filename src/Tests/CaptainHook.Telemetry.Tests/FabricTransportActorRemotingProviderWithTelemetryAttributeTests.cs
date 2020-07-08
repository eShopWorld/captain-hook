using System;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.ServiceFabric.Services.Remoting;
using Xunit;

namespace CaptainHook.Telemetry.Tests
{
    public class FabricTransportActorRemotingProviderWithTelemetryAttributeTests
    {
        [Fact, IsUnit]
        public void FabricTransportActorRemotingProviderWithTelemetryAttributeCreatesRemotingListener()
        {
            var attr = new FabricTransportActorRemotingProviderWithTelemetryAttribute();

            var dict = attr.CreateServiceRemotingListeners();

            dict.Should().NotBeEmpty();
        }

        [Fact, IsUnit]
        public void FabricTransportActorRemotingProviderWithTelemetryAttributeDoesNotAcceptV2_1Listener()
        {
            var attr = new FabricTransportActorRemotingProviderWithTelemetryAttribute
            {
                RemotingListenerVersion = RemotingListenerVersion.V2_1,
            };

            Action func = () => attr.CreateServiceRemotingListeners();

            func.Should().Throw<InvalidOperationException>();
        }
    } 
}