using System;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace CaptainHook.Common.Configuration
{
    public sealed class ServiceConfiguration
    {
        /// <summary>
        /// Get all the configuration settings
        /// </summary>
        public IConfigurationRoot Settings { get; private set; }

        public static ServiceConfiguration Load()
        {
            var result = new ServiceConfiguration();

            var kvUri = Environment.GetEnvironmentVariable(ConfigurationSettings.KeyVaultUriEnvVariable);

            result.Settings = new ConfigurationBuilder()
                .AddAzureKeyVault(
                    kvUri,
                    new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)),
                    new DefaultKeyVaultSecretManager())
                .AddEnvironmentVariables()
                .Build();

            return result;
        }
    }
}