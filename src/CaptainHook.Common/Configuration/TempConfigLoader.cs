using System;
using System.Collections.Generic;
using Azure.Security.KeyVault.Secrets;
using CaptainHook.Common.Configuration.KeyVault;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.Common.Configuration
{
    /// <summary>
    /// This is a temporary solution while KV cannot be fetched fully (as it is too big)
    /// Once EDA config is moved to DB, this can be removed and replaced with normal configuration initialisation
    /// </summary>
    public static class TempConfigLoader
    {
        public static IConfigurationRoot Load ()
        {
            var client = GetClient ();

            // these are all entry names in the keyvault header file
            var entries = new [] {
                AddEntry (client, "AzureSubscriptionId"),
                AddEntry (client, "ServiceBusNamespace"),
                AddEntry (client, "ServiceBusConnectionString"),
                AddEntry (client, "InstrumentationKey"),

                AddEntry (client, "CaptainHook--ServiceBusSubscriptionId"),
                AddEntry (client, "CaptainHook--ServiceBusConnectionString"),
                AddEntry (client, "CaptainHook--ServiceBusNamespace"),

                AddEntry (client, "CaptainHook--ApiName"),
                AddEntry (client, "CaptainHook--ApiSecret"),
                AddEntry (client, "CaptainHook--Authority"),
                AddEntry (client, "CaptainHook--RequiredScopes--1"),

                AddEntry (client, "CaptainHook--DbConfiguration--DatabaseEndpoint"),
                AddEntry (client, "CaptainHook--DbConfiguration--DatabaseKey"),
                AddEntry (client, "CaptainHook--DbConfiguration--Databases--EDA--1--CollectionName"),
                AddEntry (client, "CaptainHook--DbConfiguration--Databases--EDA--1--PartitionKey"),
            };

            var builder = new ConfigurationBuilder ();
            return builder
                .AddInMemoryCollection (entries)
                .AddEnvironmentVariables ()
                .Build ();
        }

        private static SecretClient GetClient ()
        {
            return new SecretClient (
                new Uri (Environment.GetEnvironmentVariable (ConfigurationSettings.KeyVaultUriEnvVariable)),
                new AzureServiceTokenCredential (),
                new SecretClientOptions ());
        }

        private static KeyValuePair<string, string> AddEntry (SecretClient client, string kvPath)
        {
            var configKey = kvPath.Replace ("--", ":");
            return new KeyValuePair<string, string> (configKey, GetSecretValue (client, kvPath));
        }

        private static string GetSecretValue (SecretClient client, string key)
        {
            var value = client.GetSecretAsync (key).ConfigureAwait (false).GetAwaiter ().GetResult ();
            return value?.Value?.Value;
        }
    }
}
