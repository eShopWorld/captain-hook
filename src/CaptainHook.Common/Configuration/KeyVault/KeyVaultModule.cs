using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Services.AppAuthentication;
using TokenCredential = Azure.Core.TokenCredential;

namespace CaptainHook.Common.Configuration.KeyVault
{
    public class KeyVaultModule: Module
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
                new AzureServiceTokenCredential(),
                secretClientOptions));
            builder.RegisterType<KeyVaultSecretProvider>().As<ISecretManager>();
        }

        private class AzureServiceTokenCredential: TokenCredential
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
    }
}