using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Nito.AsyncEx;
using Polly;
using Polly.Retry;
using ISubscription = Microsoft.Azure.Management.ServiceBus.Fluent.ISubscription;

namespace CaptainHook.Common.ServiceBus
{
    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class ServiceBusManager : IServiceBusManager
    {
        private const int RetryCeilingSeconds = 30;

        private readonly IMessageProviderFactory _factory;
        private readonly AsyncLazy<IServiceBusNamespace> _serviceBusNamespace;
        private readonly AsyncRetryPolicy<ITopic> _findTopicPolicy;

        public ServiceBusManager(IMessageProviderFactory factory, AsyncLazy<IServiceBusNamespace> serviceBusNamespace)
        {
            _factory = factory;
            _serviceBusNamespace = serviceBusNamespace;

            static TimeSpan ExponentialBackoff(int x) => TimeSpan.FromSeconds(Math.Clamp(Math.Pow(2, x), 0, RetryCeilingSeconds));

            _findTopicPolicy = Policy
                .HandleResult<ITopic>(b => b == null)
                .WaitAndRetryForeverAsync(ExponentialBackoff);
        }

        public async Task CreateTopicAndSubscriptionAsync(string subscriptionName, string topicName, CancellationToken cancellationToken)
        {
            var topic = await CreateTopicIfNotExistsAsync(TypeExtensions.GetEntityName(topicName), cancellationToken);
            await CreateSubscriptionIfNotExistsAsync(topic, subscriptionName, cancellationToken);
        }

        public async Task DeleteSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken)
        {
            var serviceBusNamespace = await _serviceBusNamespace;
            var topic = await FindTopicAsync(serviceBusNamespace, topicName, cancellationToken);
            if (topic != null)
            {
                await topic.Subscriptions.DeleteByNameAsync(subscriptionName, cancellationToken);
            }
        }

        private async Task<ISubscription> CreateSubscriptionIfNotExistsAsync(ITopic topic, string subscriptionName,
            CancellationToken cancellationToken)
        {
            var subscriptionsList = await topic.Subscriptions.ListAsync(cancellationToken: cancellationToken);
            var subscription = subscriptionsList.SingleOrDefault(s => string.Equals(s.Name, subscriptionName, StringComparison.OrdinalIgnoreCase));
            if (subscription != null) return subscription;

            await topic.Subscriptions
                .Define(subscriptionName.ToLowerInvariant())
                .WithMessageLockDurationInSeconds(60)
                .WithExpiredMessageMovedToDeadLetterSubscription()
                .WithMessageMovedToDeadLetterSubscriptionOnMaxDeliveryCount(10)
                .CreateAsync(cancellationToken);

            await topic.RefreshAsync(cancellationToken);
            subscriptionsList = await topic.Subscriptions.ListAsync(cancellationToken: cancellationToken);
            return subscriptionsList.Single(t => string.Equals(t.Name, subscriptionName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Setups a ServiceBus <see cref="ITopic"/> given a subscription Id, a namespace topicName and the topicName of the entity we want to work with on the topic.
        /// </summary>
        /// <param subscriptionName="topicName">The topicName of the topic entity that we are working with.</param>
        /// <param subscriptionName="cancellationToken"></param>
        /// <param name="topicName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="ITopic"/> contract for use of future operation if required.</returns>
        private async Task<ITopic> CreateTopicIfNotExistsAsync(string topicName, CancellationToken cancellationToken = default)
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

        public IMessageReceiver CreateMessageReceiver(string serviceBusConnectionString, string topicName, string subscriptionName, bool dlqMode)
        {
            return _factory.Create(serviceBusConnectionString, TypeExtensions.GetEntityName(topicName), subscriptionName.ToLowerInvariant(), dlqMode);
        }

        public string GetLockToken(Message message)
        {
            return message?.SystemProperties?.LockToken;
        }

        private static async Task<ITopic> FindTopicAsync(IServiceBusNamespace sbNamespace, string name, CancellationToken cancellationToken = default)
        {
            await sbNamespace.RefreshAsync(cancellationToken);
            var topicsList = await sbNamespace.Topics.ListAsync(cancellationToken: cancellationToken);
            return topicsList.SingleOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
