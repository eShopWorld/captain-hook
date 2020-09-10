using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using CaptainHook.Common.Configuration;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Nito.AsyncEx;

namespace CaptainHook.Common.ServiceBus
{
    public static class ServiceBusManagerAutoFacExtensions
    {
        public static ContainerBuilder ConfigureServiceBusManager(this ContainerBuilder containerBuilder, ConfigurationSettings configurationSettings)
        {
            var serviceBusNamespace = new AsyncLazy<IServiceBusNamespace>(async () =>
                await GetServiceBusNamespaceAsync(configurationSettings.AzureSubscriptionId, configurationSettings.ServiceBusNamespace));

            containerBuilder.RegisterInstance(serviceBusNamespace);

            return containerBuilder;
        } 

        private static async Task<IServiceBusNamespace> GetServiceBusNamespaceAsync(string azureSubscriptionId, string serviceBusNamespace,
            CancellationToken cancellationToken = default)
        {
            var tokenProvider = new AzureServiceTokenProvider();
            var token = await tokenProvider.GetAccessTokenAsync("https://management.core.windows.net/", string.Empty,
                cancellationToken);

            var tokenCredentials = new TokenCredentials(token);

            var client = RestClient.Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .WithCredentials(new AzureCredentials(tokenCredentials, tokenCredentials, string.Empty,
                    AzureEnvironment.AzureGlobalCloud))
                .Build();

            var sbNamespacesList = await Microsoft.Azure.Management.Fluent
                .Azure.Authenticate(client, string.Empty)
                .WithSubscription(azureSubscriptionId)
                .ServiceBusNamespaces
                .ListAsync(cancellationToken: cancellationToken);

            var sbNamespace = sbNamespacesList.SingleOrDefault(n => n.Name == serviceBusNamespace);

            if (sbNamespace == null)
            {
                throw new InvalidOperationException(
                    $"Couldn't find the service bus namespace {serviceBusNamespace} in the subscription with ID {azureSubscriptionId}");
            }

            return sbNamespace;
        }

    }
}
