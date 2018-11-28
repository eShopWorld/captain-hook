namespace CaptainHook.EventHandlerActor
{
    using System;
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

                //todo convention and clean this up
                //todo webhook--{tenantcode}-callback
                //todo webhook--{tenantcode}-authEnabled
                //todo webhook--{tenantcode}-auth--clientId
                var maxWebHookConfig = new WebHookConfig
                {
                    Uri = settings.MAXCallback,
                    AuthConfig = new AuthConfig
                    {
                        ClientId = settings.MAXClientId,
                        ClientSecret = settings.MAXClientSecret,
                        Uri = settings.MAXAuthURI
                    }
                };

                var difWebHookConfig = new WebHookConfig
                {
                    Uri = settings.DIFCallback,
                    AuthConfig = new AuthConfig
                    {
                        ClientId = settings.DIFClientId,
                        ClientSecret = settings.DIFClientSecret,
                        Uri = settings.DIFAuthURI
                    }
                };

                var eswWebHookConfig = new WebHookConfig
                {
                    Uri = settings.OMSCallback,
                    AuthConfig = new AuthConfig
                    {
                        ClientId = settings.SecurityClientId,
                        ClientSecret = settings.SecurityClientSecret,
                        Uri = settings.SecurityClientURI
                    }
                };
                
                builder.RegisterInstance(maxWebHookConfig).Named<WebHookConfig>("MAX");
                builder.RegisterInstance(difWebHookConfig).Named<WebHookConfig>("DIF");
                builder.RegisterInstance(eswWebHookConfig).Named<WebHookConfig>("ESW");


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