using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Eshopworld.Telemetry;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;
using Xunit;

namespace CaptainHook.Telemetry.Tests
{
    public class ContainerBuilderExtensionsTests
    {
        [Fact, IsUnit]
        public void AddStatefulServiceTelemetryRegistersNecessaryCompoents()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.AddStatefullServiceTelemetry();

            var container = containerBuilder.Build();
            container.IsRegistered<TelemetryClient>().Should().BeTrue();
            var initializers = container.Resolve<IEnumerable<ITelemetryInitializer>>();
            initializers.OfType<OperationCorrelationTelemetryInitializer>().Should().NotBeEmpty();
            initializers.OfType<HttpDependenciesParsingTelemetryInitializer>().Should().NotBeEmpty();
            var modules = container.Resolve<IEnumerable<ITelemetryModule>>();
            modules.OfType<DependencyTrackingTelemetryModule>().Should().NotBeEmpty();
            // It depends on an internal implementation of RegisterServiceFabricSupport
            containerBuilder.Properties.ContainsKey("__ServiceFabricRegistered").Should().BeTrue();
        }

        [Fact, IsUnit]
        public void AddStatefulServiceTelemetryRegistersTelemetryConfiguration()
        {
            var instrumentationKey = Guid.NewGuid().ToString();
            var internalKey = Guid.NewGuid().ToString();
            var telemetryInitializerMoq = new Mock<ITelemetryInitializer>();
            var telemetryModuleMoq = new Mock<ITelemetryModule>();
            telemetryModuleMoq.Setup(x => x.Initialize(It.IsAny<TelemetryConfiguration>()));
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(telemetryInitializerMoq.Object);
            containerBuilder.RegisterInstance(telemetryModuleMoq.Object);
            containerBuilder.ConfigureTelemetryKeys(instrumentationKey, internalKey);

            containerBuilder.AddStatefullServiceTelemetry();

            var container = containerBuilder.Build();
            var configuration = container.Resolve<TelemetryConfiguration>();
            configuration.InstrumentationKey.Should().Be(instrumentationKey);
            configuration.TelemetryInitializers.Should().Contain(telemetryInitializerMoq.Object);
            telemetryModuleMoq.Verify(x => x.Initialize(It.IsAny<TelemetryConfiguration>()), Times.Once);
        }
    }
}