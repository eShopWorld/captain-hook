using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using Eshopworld.Telemetry;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.EventHandlerActor
{
    internal static class Program
    {
        private const string CaptainHookConfigSection = "CaptainHook";

        /// <summary>
        ///     This is the entry point of the service host process.
        /// </summary>
        private static async Task Main()
        {
            try
            {
                var configuration = TempConfigLoader.Load ();
                var configurationSettings = configuration.Get<ConfigurationSettings>();
                var featureFlags = configuration.GetSection(CaptainHookConfigSection).Get<FeatureFlagsConfiguration>();

                var builder = new ContainerBuilder();

                builder.SetupFullTelemetry(configurationSettings.InstrumentationKey);

                builder.RegisterInstance(configurationSettings)
                    .SingleInstance();

                builder.RegisterInstance(featureFlags)
                    .SingleInstance();

                builder.RegisterActor<EventHandlerActor>();

                builder.RegisterModule<HandlerModule>();

                using (builder.Build())
                {
                    await Task.Delay(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                BigBrother.Write(e);
                throw;
            }
        }
    }
}
