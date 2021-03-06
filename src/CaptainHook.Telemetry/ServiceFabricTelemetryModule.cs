﻿using System.Fabric;
using Autofac;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.ApplicationInsights.ServiceFabric.Module;

namespace CaptainHook.Telemetry
{
    /// <summary>
    /// Registers Service Fabric components of telemetry pipeline.
    /// </summary>
    public class ServiceFabricTelemetryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x =>
            {
                var serviceContext = x.ResolveOptional<ServiceContext>();
                return FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(serviceContext);
            }).As<ITelemetryInitializer>();
            builder.Register(c => new ServiceRemotingRequestTrackingTelemetryModule { SetComponentCorrelationHttpHeaders = true }).As<ITelemetryModule>();
        }
    }
}