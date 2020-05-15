using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using Eshopworld.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Actors.Client;

namespace CaptainHook.EventReaderService
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static async Task Main()
        {
            try
            {
                var serviceConfiguration = ServiceConfiguration.Load();

                var configurationSettings = serviceConfiguration.Settings.Get<ConfigurationSettings>();

                var builder = new ContainerBuilder();
                builder.RegisterInstance(configurationSettings).SingleInstance();
                builder.RegisterType<MessageProviderFactory>().As<IMessageProviderFactory>().SingleInstance();
                builder.RegisterType<ServiceBusManager>().As<IServiceBusManager>();

                //SF Deps
                builder.Register<IActorProxyFactory>(_ => new ActorProxyFactory());

                builder.SetupFullTelemetry(configurationSettings.InstrumentationKey);
                builder.RegisterStatefulService<EventReaderService>(ServiceNaming.EventReaderServiceType);

                using (var container = builder.Build())
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
