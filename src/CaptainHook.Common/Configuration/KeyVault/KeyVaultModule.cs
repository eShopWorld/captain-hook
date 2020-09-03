using System;
using Autofac;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace CaptainHook.Common.Configuration.KeyVault
{
    public class KeyVaultModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var secretClientOptions = new SecretClientOptions
            {
                Retry =
                {
                    Delay = TimeSpan.FromSeconds(2.0),
                    MaxDelay = TimeSpan.FromSeconds(16.0),
                    MaxRetries = 3,
                    Mode = RetryMode.Exponential
                }
            };

            builder.Register(context => new SecretClient(
                new Uri(Environment.GetEnvironmentVariable(ConfigurationSettings.KeyVaultUriEnvVariable)),
                new DefaultAzureCredential(),
                secretClientOptions));
            builder.RegisterType<KeyVaultSecretProvider>().As<ISecretProvider>();
        }
    }
}