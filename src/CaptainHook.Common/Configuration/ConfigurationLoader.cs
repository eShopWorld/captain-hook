//using System;
//using Microsoft.Azure.KeyVault;
//using Microsoft.Azure.Services.AppAuthentication;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.AzureKeyVault;

//namespace CaptainHook.Common.Configuration
//{
//    public static class ConfigurationLoader
//    {
//        /// <summary>
//        /// Loads configuration from Environment and KeyVault
//        /// </summary>
//        /// <returns>All application configuration properties</returns>
//        public static IConfigurationRoot Load(string keyVaultUri)
//        {
//            var root = new ConfigurationBuilder()
//                .AddAzureKeyVault(
//                    keyVaultUri,
//                    new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)),
//                    new DefaultKeyVaultSecretManager())
//                .AddEnvironmentVariables()
//                .Build();

//            return root;
//        }
//    }
//}