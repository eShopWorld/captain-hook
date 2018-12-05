namespace CaptainHook.EventHandlerActor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Integration.ServiceFabric;
    using Common;
    using Eshopworld.Core;
    using Eshopworld.Telemetry;
    using Handlers;
    using Handlers.Authentication;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    
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
                var section = config.GetSection("webhook");
                var values = section.GetChildren().ToList();

                var list = new List<WebHookConfig>(values.Count);
                foreach (var configurationSection in values)
                {
                    var webHookConfig = new WebHookConfig
                    {
                        Name = configurationSection.Key.ToLower(),
                        DomainEvent = config[$"webhook:{configurationSection.Key}:domainevent"],
                        Uri = config[$"webhook:{configurationSection.Key}:hook"]
                    };

                    var authConfig = new AuthConfig();
                    config.GetSection($"webhook:{configurationSection.Key}:auth").Bind(authConfig);

                    webHookConfig.AuthConfig = authConfig;
                    list.Add(webHookConfig);
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

                builder.RegisterType<IHandlerFactory>().As<HandlerFactory>().SingleInstance();
                builder.RegisterType<IAuthHandlerFactory>().As<AuthHandlerFactory>().SingleInstance();

                //Register each webhook config separately for injection
                foreach (var setting in list)
                {
                    builder.RegisterInstance(setting).Named<WebHookConfig>(setting.Name);
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
    }
}