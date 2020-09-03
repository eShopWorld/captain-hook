using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Rest;
using Nito.AsyncEx;
using Polly;
using Polly.Retry;

namespace CaptainHook.Common.ServiceBus
{
    /// <inheritdoc/>
    public class ServiceBusManager : IServiceBusManager
    {
        private int _retryCeilingSeconds = 30;
        
        private readonly IMessageProviderFactory _factory;
        private readonly AsyncLazy<IServiceBusNamespace> _serviceBusNamespace;
        private readonly AsyncRetryPolicy<ITopic> _findTopicPolicy;
        private readonly Func<int, TimeSpan> _exponentialBackoff;

        public ServiceBusManager(IMessageProviderFactory factory, ConfigurationSettings configurationSettings)
        {
            _factory = factory;
            _serviceBusNamespace = new AsyncLazy<IServiceBusNamespace>(async () =>
                await GetServiceBusNamespace(configurationSettings.AzureSubscriptionId, configurationSettings.ServiceBusNamespace));

            _exponentialBackoff = x =>
            TimeSpan.FromSeconds(Math.Clamp(Math.Pow(2, x), 0, _retryCeilingSeconds));
            _findTopicPolicy = Policy
                .HandleResult<ITopic>(b => b == null)
                .WaitAndRetryForeverAsync(_exponentialBackoff);

        }

        private async Task<IServiceBusNamespace> GetServiceBusNamespace(string azureSubscriptionId, string serviceBusNamespace,
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

        public async Task CreateSubscriptionAsync(string subscriptionName, string topicName, CancellationToken cancellationToken)
        {
            await CreateTopicIfNotExists(TypeExtensions.GetEntityName(topicName), cancellationToken);
        }

        public async Task DeleteSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken)
        {
            var serviceBusNamespace = await _serviceBusNamespace;
            var topic = await FindTopicAsync(serviceBusNamespace, topicName, cancellationToken);
            await topic.Subscriptions.DeleteByNameAsync(subscriptionName, cancellationToken);
        }

        public IMessageReceiver CreateMessageReceiver(string serviceBusConnectionString, string topicName, string subscriptionName, bool dlqMode)
        {
            return _factory.Create(serviceBusConnectionString, TypeExtensions.GetEntityName(topicName), subscriptionName.ToLowerInvariant(), dlqMode);
        }

        public string GetLockToken(Message message)
        {
            return message.SystemProperties.LockToken;
        }

        private static async Task<ITopic> FindTopicAsync(IServiceBusNamespace sbNamespace, string name, CancellationToken cancellationToken = default)
        {
            await sbNamespace.RefreshAsync(cancellationToken);
            var topicsList = await sbNamespace.Topics.ListAsync(cancellationToken: cancellationToken);
            return topicsList.SingleOrDefault(t => t.Name == name.ToLowerInvariant());
        }

        /// <summary>
        /// Setups a ServiceBus <see cref="ITopic"/> given a subscription Id, a namespace topicName and the topicName of the entity we want to work with on the topic.
        /// </summary>
        /// <param name="topicName">The topicName of the topic entity that we are working with.</param>
        /// <returns>The <see cref="ITopic"/> contract for use of future operation if required.</returns>
        private async Task<ITopic> CreateTopicIfNotExists(string topicName, CancellationToken cancellationToken = default)
        {
            var serviceBusNamespace = await _serviceBusNamespace;
            var topic = await FindTopicAsync(serviceBusNamespace, topicName, cancellationToken);

            if (topic != null) return topic;

            await serviceBusNamespace.Topics
                .Define(topicName.ToLowerInvariant())
                .WithDuplicateMessageDetection(TimeSpan.FromMinutes(10))
                .CreateAsync(cancellationToken);

            return await _findTopicPolicy.ExecuteAsync(ct => FindTopicAsync(serviceBusNamespace, topicName, ct), cancellationToken);
        }

    }
}
