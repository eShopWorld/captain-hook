using System;
using Autofac;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace CaptainHook.Common.Configuration.KeyVault
{
    public class KeyVaultModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(context => new SecretClient(
                new Uri(Environment.GetEnvironmentVariable(ConfigurationSettings.KeyVaultUriEnvVariable)),
                new DefaultAzureCredential()))
                .SingleInstance();
            builder.RegisterType<KeyVaultSecretManager>().As<ISecretManager>();
        }
    }
}