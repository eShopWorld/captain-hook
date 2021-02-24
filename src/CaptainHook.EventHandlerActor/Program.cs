using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers;
using Eshopworld.DevOps;
using Eshopworld.Telemetry;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.EventHandlerActor
{
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static async Task Main()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                   .UseDefaultConfigs()
                   .AddKeyVaultSecrets(new Dictionary<string, string>
                   {
                        {"cm--ai-telemetry--instrumentation", "Telemetry:InstrumentationKey"},
                        {"cm--ai-telemetry--internal", "Telemetry:InternalKey"},
                   }).Build();

                var telemetrySettings = configuration.GetSection("Telemetry").Get<TelemetrySettings>();
                var loggingConfiguration = configuration.GetSection(nameof(LoggingConfiguration)).Get<LoggingConfiguration>();

                var builder = new ContainerBuilder();
                builder.SetupFullTelemetry(telemetrySettings.InstrumentationKey, telemetrySettings.InternalKey);
                builder.RegisterInstance(loggingConfiguration).SingleInstance();
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
