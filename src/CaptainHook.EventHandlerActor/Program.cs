using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Common.Authentication;
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
                var section = config.GetSection("event");
                var values = section.GetChildren().ToList();

                var eventHandlerList = new List<EventHandlerConfig>();
                var webhookList = new List<WebhookConfig>(values.Count);
                foreach (var configurationSection in values)
                {
                    //temp work around until config comes in through the API
                    var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();
                    eventHandlerList.Add(eventHandlerConfig);

                    if (eventHandlerConfig.WebHookConfig != null)
                    {
                        if (eventHandlerConfig.WebHookConfig.AuthenticationConfig.Type == AuthenticationType.Basic)
                        {
                            var basicAuthenticationConfig = new BasicAuthenticationConfig
                            {
                                Username = configurationSection["webhookconfig:authenticationconfig:username"],
                                Password = configurationSection["webhookconfig:authenticationconfig:password"]
                            };
                            eventHandlerConfig.WebHookConfig.AuthenticationConfig = basicAuthenticationConfig;
                        }

                        if (eventHandlerConfig.WebHookConfig.AuthenticationConfig.Type == AuthenticationType.OAuth)
                        {
                            eventHandlerConfig.WebHookConfig.AuthenticationConfig = ParseOAuthAuthenticationConfig(configurationSection.GetSection("webhookconfig:authenticationconfig"));
                        }

                        if (eventHandlerConfig.WebHookConfig.AuthenticationConfig.Type == AuthenticationType.Custom)
                        {
                            eventHandlerConfig.WebHookConfig.AuthenticationConfig = ParseOAuthAuthenticationConfig(configurationSection.GetSection("webhookconfig:authenticationconfig"));
                            eventHandlerConfig.WebHookConfig.AuthenticationConfig.Type = AuthenticationType.Custom;
                        }

                        webhookList.Add(eventHandlerConfig.WebHookConfig);
                    }

                    if (eventHandlerConfig.CallBackEnabled)
                    {
                        if (eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type == AuthenticationType.Basic)
                        {
                            var basicAuthenticationConfig = new BasicAuthenticationConfig
                            {
                                Username = configurationSection["webhookconfig:authenticationconfig:username"],
                                Password = configurationSection["webhookconfig:authenticationconfig:password"]
                            };
                            eventHandlerConfig.CallbackConfig.AuthenticationConfig = basicAuthenticationConfig;
                        }

                        if (eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type == AuthenticationType.OAuth)
                        {
                            eventHandlerConfig.CallbackConfig.AuthenticationConfig = ParseOAuthAuthenticationConfig(configurationSection.GetSection("callbackconfig:authenticationconfig"));
                        }

                        if (eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type == AuthenticationType.Custom)
                        {
                            eventHandlerConfig.CallbackConfig.AuthenticationConfig = ParseOAuthAuthenticationConfig(configurationSection.GetSection("callbackconfig:authenticationconfig"));
                            eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type = AuthenticationType.Custom;
                        }

                        webhookList.Add(eventHandlerConfig.CallbackConfig);
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
                builder.RegisterType<AuthenticationHandlerFactory>().As<IAuthHandlerFactory>().SingleInstance();

                //Register each webhook authenticationConfig separately for injection
                foreach (var setting in eventHandlerList)
                {
                    builder.RegisterInstance(setting).Named<EventHandlerConfig>(setting.Name);
                }

                foreach (var webhookConfig in webhookList)
                {
                    builder.RegisterInstance(webhookConfig).Named<WebhookConfig>(webhookConfig.Name);
                    builder.RegisterInstance(new HttpClient()).Named<HttpClient>(webhookConfig.Name).SingleInstance();
                }

                builder.RegisterServiceFabricSupport();
                builder.RegisterActor<EventHandlerActor>();

                using (builder.Build())
                {
                    await Task.Delay(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                BigBrother.Write(e);
                throw;
            }
        }

        /// <summary>
        /// Hack to parse out the config types, won't be needed after api configuration
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        private static OAuthAuthenticationConfig ParseOAuthAuthenticationConfig(IConfiguration configurationSection)
        {
            var oauthAuthenticationConfig = new OAuthAuthenticationConfig
            {
                ClientId = configurationSection["clientid"],
                ClientSecret = configurationSection["clientsecret"],
                Uri = configurationSection["uri"],
                Scopes = configurationSection["scopes"].Split(" ")
            };

            var refresh = configurationSection["refresh"];
            if (string.IsNullOrWhiteSpace(refresh))
            {
                oauthAuthenticationConfig.RefreshBeforeInSeconds = 10;
            }
            else
            {
                if (int.TryParse(refresh, out var refreshValue))
                {
                    oauthAuthenticationConfig.RefreshBeforeInSeconds = refreshValue;
                }
            }

            return oauthAuthenticationConfig;
        }
    }
}
