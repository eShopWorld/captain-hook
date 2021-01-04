using Autofac;
using CaptainHook.Telemetry;
using Eshopworld.Telemetry;
using Eshopworld.Telemetry.Configuration;

namespace CaptainHook.Common.Telemetry
{
    public static class TelemetryExtensions
    {
        public static void SetupFullTelemetry(this ContainerBuilder builder, string instrumentationKey, string internalKey)
        {
            builder.ConfigureTelemetryKeys(instrumentationKey, internalKey);
            builder.AddStatefullServiceTelemetry();

            builder.RegisterModule<TelemetryModule>();
            builder.RegisterModule<ServiceFabricTelemetryModule>();
        }
    }
}
