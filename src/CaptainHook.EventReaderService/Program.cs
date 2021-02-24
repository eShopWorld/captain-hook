using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Common;
using CaptainHook.Common.ServiceBus;
using CaptainHook.Common.Telemetry;
using Eshopworld.DevOps;
using Eshopworld.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Actors.Client;

namespace CaptainHook.EventReaderService
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
                        {"cm--sb-connection--esw-eda", $"{nameof(ServiceBusSettings)}:{nameof(ServiceBusSettings.ConnectionString)}"},
                    }).Build();


                var telemetrySettings = configuration.GetSection("Telemetry").Get<TelemetrySettings>();
                var serviceBusSettings = configuration.GetSection(nameof(ServiceBusSettings)).Get<ServiceBusSettings>();

                var builder = new ContainerBuilder();
                builder.RegisterInstance(serviceBusSettings).SingleInstance();
                builder.RegisterType<MessageProviderFactory>().As<IMessageProviderFactory>().SingleInstance();
                builder.RegisterType<ServiceBusManager>().As<IServiceBusManager>();
                builder.RegisterType<MessageLockDurationCalculator>().As<IMessageLockDurationCalculator>().SingleInstance();

                //SF Deps
                builder.Register<IActorProxyFactory>(_ => new ActorProxyFactory());

                builder.SetupFullTelemetry(telemetrySettings.InstrumentationKey, telemetrySettings.InternalKey);
                builder.RegisterStatefulService<EventReaderService>(ServiceNaming.EventReaderServiceType);

                using (builder.Build())
                {
                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, ServiceNaming.EventReaderServiceType);
                    await Task.Delay(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                BigBrother.Write(e);
                throw;
            }
        }
    }
}
