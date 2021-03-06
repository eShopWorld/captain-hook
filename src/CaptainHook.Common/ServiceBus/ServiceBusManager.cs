using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using Eshopworld.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
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
        private readonly IBigBrother _bigBrother;
        private readonly AsyncRetryPolicy<ITopic> _findTopicPolicy;
        private readonly ServiceBusSettings _serviceBusSettings;

        public ServiceBusManager(
            IMessageProviderFactory factory,
            IBigBrother bigBrother,
            ServiceBusSettings serviceBusSettings)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
            _serviceBusSettings = serviceBusSettings ?? throw new ArgumentNullException(nameof(serviceBusSettings));

            static TimeSpan ExponentialBackoff(int x) => TimeSpan.FromSeconds(Math.Clamp(Math.Pow(2, x), 0, RetryCeilingSeconds));

            _findTopicPolicy = Policy
                .HandleResult<ITopic>(b => b == null)
                .WaitAndRetryForeverAsync(ExponentialBackoff);
        }

        public async Task CreateTopicAndSubscriptionAsync(string subscriptionName, string topicName, int maxDeliveryCount, int messageLockDurationInSeconds, CancellationToken cancellationToken)
        {
            var topic = await CreateTopicIfNotExistsAsync(TypeExtensions.GetEntityName(topicName), cancellationToken);
            await CreateSubscriptionIfNotExistsAsync(topic, subscriptionName, maxDeliveryCount, messageLockDurationInSeconds, cancellationToken);
        }

        public async Task DeleteSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken)
        {
            var serviceBusNamespace = await GetServiceBusNamespaceAsync(cancellationToken);
            var topic = await FindTopicAsync(serviceBusNamespace, TypeExtensions.GetEntityName(topicName), cancellationToken);
            if (topic != null)
            {
                await topic.Subscriptions.DeleteByNameAsync(subscriptionName, cancellationToken);
            }
        }

        private async Task<ISubscription> CreateSubscriptionIfNotExistsAsync(
            ITopic topic,
            string subscriptionName,
            int maxDeliveryCount,
            int messageLockDurationInSeconds,
            CancellationToken cancellationToken)
        {
            if (maxDeliveryCount <= 0)
            {
                maxDeliveryCount = WebhookConfig.DefaultMaxDeliveryCount;
            }

            var subscriptionsList = await topic.Subscriptions.ListAsync(cancellationToken: cancellationToken);
            var subscription = subscriptionsList.SingleOrDefault(s => string.Equals(s.Name, subscriptionName, StringComparison.OrdinalIgnoreCase));
            if (subscription != null)
            {
                await subscription.Update()
                    .WithMessageLockDurationInSeconds(messageLockDurationInSeconds)
                    .WithMessageMovedToDeadLetterQueueOnMaxDeliveryCount(maxDeliveryCount)
                    .ApplyAsync(cancellationToken);

                return subscription;
            }

            try
            {
                await topic.Subscriptions
                    .Define(subscriptionName.ToLowerInvariant())
                    .WithMessageLockDurationInSeconds(messageLockDurationInSeconds)
                    .WithExpiredMessageMovedToDeadLetterSubscription()
                    .WithMessageMovedToDeadLetterSubscriptionOnMaxDeliveryCount(maxDeliveryCount)
                    .CreateAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
            }

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
            var serviceBusNamespace = await GetServiceBusNamespaceAsync(cancellationToken);
            var topic = await FindTopicAsync(serviceBusNamespace, topicName, cancellationToken);

            if (topic != null) return topic;

            try
            {
                await serviceBusNamespace.Topics
                    .Define(topicName.ToLowerInvariant())
                    .WithDuplicateMessageDetection(TimeSpan.FromMinutes(10))
                    .CreateAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
            }

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
            try
            {
                var topic = await sbNamespace.Topics.GetByNameAsync(name, cancellationToken);
                return topic;
            }
            catch (CloudException cex) when (cex.Body.Code == "NotFound")
            {
                return null;
            }
        }

        private async Task<IServiceBusNamespace> GetServiceBusNamespaceAsync(CancellationToken cancellationToken = default)
        {
            var tokenProvider = new AzureServiceTokenProvider();
            var token = await tokenProvider.GetAccessTokenAsync("https://management.core.windows.net/", string.Empty,
                cancellationToken);

            var tokenCredentials = new TokenCredentials(token);

            var client = RestClient.Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .WithCredentials(new AzureCredentials(tokenCredentials, tokenCredentials, string.Empty, AzureEnvironment.AzureGlobalCloud))
                .Build();

            var sbNamespacesList = await Microsoft.Azure.Management.Fluent
                .Azure.Authenticate(client, string.Empty)
                .WithSubscription(_serviceBusSettings.SubscriptionId)
                .ServiceBusNamespaces
                .ListAsync(cancellationToken: cancellationToken);

            var sbNamespaceName = RetrieveNamespace(_serviceBusSettings.ConnectionString);
            var sbNamespace = sbNamespacesList.SingleOrDefault(n => n.Name == sbNamespaceName);

            if (sbNamespace == null)
            {
                throw new InvalidOperationException(
                    $"Couldn't find the service bus namespace {sbNamespaceName} in the subscription with ID {_serviceBusSettings.SubscriptionId}.");
            }

            return sbNamespace;
        }

        private string RetrieveNamespace(string sbConnectionString)
        {
            return NamespaceRegex.Matches(sbConnectionString).FirstOrDefault()?.Groups["namespace"]?.Value;
        }

        private static readonly Regex NamespaceRegex = new Regex(@"sb:\/\/(?<namespace>.*)\.servicebus", RegexOptions.Compiled);
    }
}
