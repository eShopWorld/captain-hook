using Autofac;
using CaptainHook.Cli.Services;
using Eshopworld.DevOps;
using Eshopworld.Telemetry;
using Eshopworld.Telemetry.Configuration;
using Microsoft.ApplicationInsights;
using System;

namespace CaptainHook.Cli
{
    public class CoreModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //var config = EswDevOpsSdk.BuildConfiguration();
            //builder.ConfigureTelemetryKeys(config["BBInstrumentationKey"], config["BBInstrumentationKey"]);
            //builder.RegisterModule<TelemetryModule>();
            //builder.RegisterType<TelemetryClient>().SingleInstance();
            builder.RegisterType<PathService>().SingleInstance();
        }
    }
}
