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
                var endpointList = new Dictionary<string, WebhookConfig>(values.Count);
                foreach (var configurationSection in values)
                {
                    //temp work around until config comes in through the API
                    var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();
                    eventHandlerList.Add(eventHandlerConfig);

                    if (eventHandlerConfig.WebhookConfig != null)
                    {
                        if (eventHandlerConfig.WebhookConfig.AuthenticationConfig.Type == AuthenticationType.Basic)
                        {
                            var basicAuthenticationConfig = new BasicAuthenticationConfig
                            {
                                Username = configurationSection["webhookconfig:authenticationconfig:username"],
                                Password = configurationSection["webhookconfig:authenticationconfig:password"]
                            };
                            eventHandlerConfig.WebhookConfig.AuthenticationConfig = basicAuthenticationConfig;
                        }

                        if (eventHandlerConfig.WebhookConfig.AuthenticationConfig.Type == AuthenticationType.OIDC)
                        {
                            eventHandlerConfig.WebhookConfig.AuthenticationConfig = ParseOidcAuthenticationConfig(configurationSection.GetSection("webhookconfig:authenticationconfig"));
                        }

                        if (eventHandlerConfig.WebhookConfig.AuthenticationConfig.Type == AuthenticationType.Custom)
                        {
                            eventHandlerConfig.WebhookConfig.AuthenticationConfig = ParseOidcAuthenticationConfig(configurationSection.GetSection("webhookconfig:authenticationconfig"));
                            eventHandlerConfig.WebhookConfig.AuthenticationConfig.Type = AuthenticationType.Custom;
                        }

                        webhookList.Add(eventHandlerConfig.WebhookConfig);
                        AddEndpoints(eventHandlerConfig.WebhookConfig, endpointList);
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

                        if (eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type == AuthenticationType.OIDC)
                        {
                            eventHandlerConfig.CallbackConfig.AuthenticationConfig = ParseOidcAuthenticationConfig(configurationSection.GetSection("callbackconfig:authenticationconfig"));
                        }

                        if (eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type == AuthenticationType.Custom)
                        {
                            eventHandlerConfig.CallbackConfig.AuthenticationConfig = ParseOidcAuthenticationConfig(configurationSection.GetSection("callbackconfig:authenticationconfig"));
                            eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type = AuthenticationType.Custom;
                        }

                        webhookList.Add(eventHandlerConfig.CallbackConfig);
                        AddEndpoints(eventHandlerConfig.CallbackConfig, endpointList);
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

                //Register each webhook authenticationConfig separately for injection
                foreach (var setting in eventHandlerList)
                {
                    builder.RegisterInstance(setting).Named<EventHandlerConfig>(setting.Name);
                }

                foreach (var webhookConfig in webhookList)
                {
                    builder.RegisterInstance(webhookConfig).Named<WebhookConfig>(webhookConfig.Name);
                }

                //todo use http client factory and do not manage it here
                //creates a list of unique endpoint and the corresponding http client for each
                foreach (var (key, value) in endpointList)
                {
                    var httpClient = new HttpClient { Timeout = value.Timeout };
                    builder.RegisterInstance(httpClient).Named<HttpClient>(key).SingleInstance();
                    builder.RegisterInstance(value).Named<WebhookConfig>(key);
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
        /// Creates a list of unique endpoints which are used for authentication sharing and pooling
        /// </summary>
        /// <param name="webhookConfig"></param>
        /// <param name="endpointList"></param>
        private static void AddEndpoints(WebhookConfig webhookConfig, IDictionary<string, WebhookConfig> endpointList)
        {
            //creates a list of endpoints so they can be shared for authentication and http pooling
            if (string.IsNullOrWhiteSpace(webhookConfig.Uri))
            {
                if (!webhookConfig.WebhookRequestRules.Any(r => r.Routes.Any()))
                {
                    return;
                }

                foreach (var rules in webhookConfig.WebhookRequestRules)
                {
                    foreach (var rule in rules.Routes)
                    {
                        if (string.IsNullOrWhiteSpace(rule.Uri))
                        {
                            continue;
                        }

                        SafeAdd(endpointList, rule);
                    }
                }
            }
            else
            {
                SafeAdd(endpointList, webhookConfig);
            }
        }

        /// <summary>
        /// Safe adds to the dictionary if it does not already exist
        /// </summary>
        /// <param name="endpointList"></param>
        /// <param name="rule"></param>
        private static void SafeAdd(IDictionary<string, WebhookConfig> endpointList, WebhookConfig rule)
        {
            var uri = new Uri(rule.Uri);
            if (!endpointList.ContainsKey(uri.Host))
            {
                endpointList.Add(uri.Host, rule);
            }
        }

        /// <summary>
        /// Hack to parse out the config types, won't be needed after api configuration
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        private static OidcAuthenticationConfig ParseOidcAuthenticationConfig(IConfiguration configurationSection)
        {
            var oauthAuthenticationConfig = new OidcAuthenticationConfig
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
