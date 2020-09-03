using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Rest;
using Polly;
using Polly.Retry;

namespace CaptainHook.Common.ServiceBus
{
    /// <summary>
    /// Contains extensions to the ServiceBus Fluent SDK: <see cref="Microsoft.Azure.Management.ServiceBus.Fluent"/>.
    /// </summary>
    public static class ServiceBusNamespaceExtensions
    {
        private static int _retryCeilingSeconds = 30;
        private static Func<int, TimeSpan> _exponentialBackoff = x =>
            TimeSpan.FromSeconds(Math.Clamp(Math.Pow(2, x), 0, _retryCeilingSeconds));
        private static AsyncRetryPolicy<ITopic> _findTopicPolicy = Policy
            .HandleResult<ITopic>(b => b == null)
            .WaitAndRetryForeverAsync(_exponentialBackoff);

        /// <summary>
        /// Creates a specific topic if it doesn't exist in the target namespace.
        /// </summary>
        /// <param name="sbNamespace">The <see cref="IServiceBusNamespace"/> where we are creating the topic in.</param>
        /// <param name="name">The name of the topic that we are looking for.</param>
        /// <returns>The <see cref="ITopic"/> entity object that references the Azure topic.</returns>
        public static async Task<ITopic> CreateTopicIfNotExists(this IServiceBusNamespace sbNamespace, string name, CancellationToken cancellationToken = default)
        {
            var topic = await FindTopicAsync(sbNamespace, name, cancellationToken);

            if (topic != null) return topic;

            await sbNamespace.Topics
                             .Define(name.ToLowerInvariant())
                             .WithDuplicateMessageDetection(TimeSpan.FromMinutes(10))
                             .CreateAsync(cancellationToken);

            return await _findTopicPolicy.ExecuteAsync(ct => FindTopicAsync(sbNamespace, name, ct), cancellationToken);

        }

        private static async Task<ITopic> FindTopicAsync(IServiceBusNamespace sbNamespace, string name, CancellationToken cancellationToken = default)
        {
            await sbNamespace.RefreshAsync(cancellationToken);
            var topicsList = await sbNamespace.Topics.ListAsync(cancellationToken: cancellationToken);
            return topicsList.SingleOrDefault(t => t.Name == name.ToLowerInvariant());
        }

        /// <summary>
        /// Creates a specific subscription to a topic if it doesn't exist yet.
        /// </summary>
        /// <param name="topic">The <see cref="ITopic"/> that we are subscribing to.</param>
        /// <param name="name">The name of the subscription we are doing on the <see cref="ITopic"/>.</param>
        /// <returns>The <see cref="Microsoft.Azure.Management.ServiceBus.Fluent.ISubscription"/> entity object that references the subscription.</returns>
        public static async Task<Microsoft.Azure.Management.ServiceBus.Fluent.ISubscription> CreateSubscriptionIfNotExists(this ITopic topic, string name, CancellationToken cancellationToken)
        {
            await topic.RefreshAsync(cancellationToken);

            var subscriptionsList = await topic.Subscriptions.ListAsync(cancellationToken: cancellationToken);
            var subscription = subscriptionsList.SingleOrDefault(s => s.Name == name.ToLowerInvariant());
            if (subscription != null) return subscription;

            await topic.Subscriptions
                       .Define(name.ToLowerInvariant())
                       .WithMessageLockDurationInSeconds(60)
                       .WithExpiredMessageMovedToDeadLetterSubscription()
                       .WithMessageMovedToDeadLetterSubscriptionOnMaxDeliveryCount(10)
                       .CreateAsync(cancellationToken);

            await topic.RefreshAsync(cancellationToken);
            subscriptionsList = await topic.Subscriptions.ListAsync(cancellationToken: cancellationToken);
            return subscriptionsList.Single(t => t.Name == name.ToLowerInvariant());
        }

        /// <summary>
        /// Setups a ServiceBus <see cref="ITopic"/> given a subscription Id, a namespace name and the name of the entity we want to work with on the topic.
        /// </summary>
        /// <param name="azureSubscriptionId">The Azure subscription ID where the topic exists.</param>
        /// <param name="serviceBusNamespace">The Azure ServiceBus namespace name.</param>
        /// <param name="entityName">The name of the topic entity that we are working with.</param>
        /// <returns>The <see cref="ITopic"/> contract for use of future operation if required.</returns>
        public static async Task<ITopic> SetupTopic(string azureSubscriptionId, string serviceBusNamespace, string entityName, CancellationToken cancellationToken)
        {
            var sbNamespace = await GetServiceBusNamespace(azureSubscriptionId, serviceBusNamespace, cancellationToken);

            return await sbNamespace.CreateTopicIfNotExists(entityName, cancellationToken);
        }

        private static async Task<IServiceBusNamespace> GetServiceBusNamespace(string azureSubscriptionId, string serviceBusNamespace,
            CancellationToken cancellationToken)
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

        public static async Task DeleteSubscription(string azureSubscriptionId, string sbNamespace, string topicName, string subscriptionName,
            CancellationToken cancellationToken)
        {
            var sbNamespaceObj = await GetServiceBusNamespace(azureSubscriptionId, sbNamespace, cancellationToken);
            var topic = await FindTopicAsync(sbNamespaceObj, topicName, cancellationToken);
            await topic.Subscriptions.DeleteByNameAsync(subscriptionName, cancellationToken);
        }
    }
}