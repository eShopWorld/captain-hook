using System;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.Common.Configuration
{
    public class KeyVaultConfigurationLoader : IKeyVaultConfigurationLoader
    {
        private readonly SecretClient _secretClient;

        public KeyVaultConfigurationLoader(SecretClient secretClient)
        {
            _secretClient = secretClient ?? throw new ArgumentNullException(nameof(secretClient));
        }

        /// <summary>
        /// Loads events configuration from KeyVault
        /// </summary>
        /// <returns>Only events configuration</returns>
        public IConfigurationRoot Load(string keyVaultUri)
        {
            var root = new ConfigurationBuilder()
                .AddAzureKeyVault(_secretClient, new KeyVaultSecretManager())
                .Build();

            return root;
        }
    }
}