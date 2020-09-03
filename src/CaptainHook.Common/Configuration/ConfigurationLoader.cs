using System;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.Common.Configuration
{
    public static class ConfigurationLoader
    {
        /// <summary>
        /// Loads configuration from Environment and KeyVault
        /// </summary>
        /// <returns>All application configuration properties</returns>
        public static IConfigurationRoot Load(string keyVaultUri)
        {
            var root = new ConfigurationBuilder()
                .AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential())
                .AddEnvironmentVariables()
                .Build();

            return root;
        }
    }
}