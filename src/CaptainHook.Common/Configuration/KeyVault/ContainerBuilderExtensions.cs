using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using TokenCredential = Azure.Core.TokenCredential;

namespace CaptainHook.Common.Configuration.KeyVault
{
    [ExcludeFromCodeCoverage]
    internal class AzureServiceTokenCredential : TokenCredential
    {
        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var token = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://vault.azure.net", string.Empty);
            return new AccessToken(token, DateTimeOffset.UtcNow.AddMinutes(5.0));
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return GetTokenAsync(requestContext, cancellationToken).Result;
        }
    }

    [ExcludeFromCodeCoverage]
    public static class ContainerBuilderExtensions
    {
        public static void RegisterKeyVaultSecretProvider(this ContainerBuilder builder, IConfigurationRoot configuration)
        {
            var keyVaultUrl = configuration.GetValue<string>("KEYVAULT_URL");

            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                var instanceName = configuration.GetValue<string>("KeyVaultInstanceName");
                keyVaultUrl = $"https://{instanceName}.vault.azure.net";
            }

            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new InvalidOperationException("KeyVault Uri or KeyVaultInstanceName must be provided in config");
            }

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

            builder.Register(context => new SecretClient(new Uri(keyVaultUrl), new AzureServiceTokenCredential(), secretClientOptions));
            builder.RegisterType<KeyVaultSecretProvider>().As<ISecretProvider>();
        }
    }
}