using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Telemetry;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

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
                var configuration = Configuration.Load();

                
                var configurationSettings = new ConfigurationSettings();
                configuration.Settings.Bind(configurationSettings);

                var builder = new ContainerBuilder();

                builder.SetupFullTelemetry(configurationSettings.InstrumentationKey);

                builder.RegisterInstance(configurationSettings)
                    .SingleInstance();

                builder.RegisterType<EventHandlerFactory>().As<IEventHandlerFactory>().SingleInstance();
                builder.RegisterType<AuthenticationHandlerFactory>().As<IAuthenticationHandlerFactory>().SingleInstance();
                builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>();
                builder.RegisterType<RequestLogger>().As<IRequestLogger>();
                builder.RegisterType<RequestBuilder>().As<IRequestBuilder>();

                //Register each webhook authenticationConfig separately for injection
                foreach (var subscriber in configuration.SubscriberConfigurations)
                {
                    builder.RegisterInstance(subscriber.Value).Named<SubscriberConfiguration>(subscriber.Key);
                }

                foreach (var webhookConfig in configuration.WebhookConfigurations)
                {
                    builder.RegisterInstance(webhookConfig).Named<WebhookConfig>(webhookConfig.Name.ToLowerInvariant());
                }

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
