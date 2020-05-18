using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Telemetry;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.EventHandlerActor
{
    internal static class Program
    {
        /// <summary>
        ///     This is the entry point of the service host process.
        /// </summary>
        private static async Task Main()
        {
            try
            {
                var configuration = ConfigurationLoader.Load();
                var configurationSettings = configuration.Get<ConfigurationSettings>();

                var builder = new ContainerBuilder();

                builder.SetupFullTelemetry(configurationSettings.InstrumentationKey);

                builder.RegisterInstance(configurationSettings)
                    .SingleInstance();

                builder.RegisterType<EventHandlerFactory>().As<IEventHandlerFactory>().SingleInstance();
                builder.RegisterType<AuthenticationHandlerFactory>().As<IAuthenticationHandlerFactory>().SingleInstance();
                builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>();
                builder.RegisterType<RequestLogger>().As<IRequestLogger>();
                builder.RegisterType<RequestBuilder>().As<IRequestBuilder>();

                builder.RegisterActor<EventHandlerActor>();

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
