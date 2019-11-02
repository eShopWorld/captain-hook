using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
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
                var kvUri = Environment.GetEnvironmentVariable(ConfigurationSettings.KeyVaultUriEnvVariable);

                var config = new ConfigurationBuilder().AddAzureKeyVault(
                    kvUri,
                    new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider()
                            .KeyVaultTokenCallback)),
                    new DefaultKeyVaultSecretManager()).Build();

                //autowire up configs in keyvault to webhooks
                var values = config.GetSection("event").GetChildren().ToList();

                var subscriberConfigurations = new Dictionary<string, SubscriberConfiguration>();
                var webhookList = new List<WebhookConfig>(values.Count);
                var endpointList = new Dictionary<string, WebhookConfig>(values.Count);

                foreach (var configurationSection in values)
                {
                    //temp work around until config comes in through the API
                    var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();

                    foreach (var subscriber in eventHandlerConfig.AllSubscribers)
                    {                    
                        subscriberConfigurations.Add(
                            SubscriberConfiguration.Key(eventHandlerConfig.Type, subscriber.SubscriberName),
                            subscriber);

                        var path = "webhookconfig";
                        ConfigParser.ParseAuthScheme(subscriber, configurationSection, $"{path}:authenticationconfig");
                        subscriber.EventType = eventHandlerConfig.Type;
                        webhookList.Add(subscriber);
                        ConfigParser.AddEndpoints(subscriber, endpointList, configurationSection, path);

                        if (subscriber.Callback != null)
                        {
                            path = "callbackconfig";
                            ConfigParser.ParseAuthScheme(subscriber.Callback, configurationSection, $"{path}:authenticationconfig");
                            subscriber.Callback.EventType = eventHandlerConfig.Type;
                            webhookList.Add(subscriber.Callback);
                            ConfigParser.AddEndpoints(subscriber.Callback, endpointList, configurationSection, path);
                        }
                    }
                }

                var settings = new ConfigurationSettings();
                config.Bind(settings);

                var bb = new BigBrother(settings.InstrumentationKey, settings.InstrumentationKey);
                bb.UseEventSourceSink().ForExceptions();

                var builder = new ContainerBuilder();
                builder.RegisterInstance(bb)
                    .As<IBigBrother>()
                    .SingleInstance();

                builder.RegisterInstance(settings)
                    .SingleInstance();

                builder.RegisterType<EventHandlerFactory>().As<IEventHandlerFactory>().SingleInstance();
                builder.RegisterType<AuthenticationHandlerFactory>().As<IAuthenticationHandlerFactory>().SingleInstance();
                builder.RegisterType<HttpClientFactory>().As<Handlers.IHttpClientFactory>();
                builder.RegisterType<RequestLogger>().As<IRequestLogger>();
                builder.RegisterType<RequestBuilder>().As<IRequestBuilder>();

                //Register each webhook authenticationConfig separately for injection
                foreach (var subscriber in subscriberConfigurations)
                {
                    builder.RegisterInstance(subscriber.Value).Named<SubscriberConfiguration>(subscriber.Key);
                }

                foreach (var webhookConfig in webhookList)
                {
                    builder.RegisterInstance(webhookConfig).Named<WebhookConfig>(webhookConfig.Name);
                }

                ////creates a list of unique endpoint and the corresponding http client for each which can be selected at runtime
                //foreach (var (key, value) in endpointList)
                //{
                //    builder.RegisterInstance(value).Named<WebhookConfig>(key).SingleInstance();
                //}

                builder.RegisterServiceFabricSupport();
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
