using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaptainHook.Common.Configuration
{
    public sealed class Configuration
    {
        /// <summary>
        /// Get all the configuration settings
        /// </summary>
        public IConfigurationRoot Settings { get; private set; }

        /// <summary>
        /// Get the domain events configuration
        /// </summary>
        public IList<EventHandlerConfig> EventHandlers { get; private set; }

        private Configuration()
        {
        }

        /// <summary>
        /// Load configuration settings and domain events 
        /// </summary>
        /// <returns>An instance holding the configuration settings and domain events</returns>
        public static Configuration Load()
        {
            var result = new Configuration();

            result.LoadFromKeyVaultAndEnvironment();
            // result.LoadFromCosmosDb();

            return result;
        }

        private void LoadFromKeyVaultAndEnvironment()
        {
            var kvUri = Environment.GetEnvironmentVariable(ConfigurationSettings.KeyVaultUriEnvVariable);

            this.Settings = new ConfigurationBuilder()
                .AddAzureKeyVault(
                    kvUri,
                    new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)),
                    new DefaultKeyVaultSecretManager())
                .AddEnvironmentVariables()
                .Build();

            this.EventHandlers = this.Settings.GetSection("event")
                .Get<IEnumerable<EventHandlerConfig>>()
                .ToList();
        }
    }
}
