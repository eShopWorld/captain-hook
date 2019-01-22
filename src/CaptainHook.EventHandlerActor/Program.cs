namespace CaptainHook.EventHandlerActor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
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

                var list = new List<EventHandlerConfig>(values.Count);
                foreach (var configurationSection in values)
                {
                    var webHookConfig = config.GetSection($"webhook:{configurationSection.Key}").Get<EventHandlerConfig>();
                    
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

                builder.RegisterType<HandlerFactory>().As<IHandlerFactory>().SingleInstance();
                builder.RegisterType<AuthenticationHandlerFactory>().As<IAuthHandlerFactory>().SingleInstance();

                //Register each webhook config separately for injection
                foreach (var setting in list)
                {
                    builder.RegisterInstance(setting).Named<EventHandlerConfig>(setting.Name);
                    builder.RegisterInstance(new HttpClient()).Named<HttpClient>(setting.Name).SingleInstance();
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
