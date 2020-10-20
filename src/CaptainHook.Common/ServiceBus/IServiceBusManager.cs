using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace CaptainHook.Common.ServiceBus
{
    /// <summary>
    /// A wrapper for ServiceBus Functions
    /// </summary>
    public interface IServiceBusManager
    {
        /// <summary>
        /// Creates a topic based on the specified type and subscription to that topic with the specify type
        /// </summary>
        /// <param name="subscriptionName"></param>
        /// <param name="topicName"></param>
        /// <param name="maxDeliveryCount"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CreateTopicAndSubscriptionAsync(string subscriptionName, string topicName, int maxDeliveryCount, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a message receiver to the service bus namespace
        /// </summary>
        /// <param name="serviceBusConnectionString"></param>
        /// <param name="topicName"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="dlqMode">true if meant to subscribe to DLQ messages</param>
        /// <returns></returns>
        IMessageReceiver CreateMessageReceiver(string serviceBusConnectionString, string topicName, string subscriptionName, bool dlqMode);

        /// <summary>
        /// Abstraction around the ServiceBusMessage to get lock token
        /// MS have this pretty locked down so there isn't much we can do with mocking or reflection on the Message itself.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string GetLockToken(Message message);
        
        /// <summary>
        /// Deletes the subscription from the topic on the current ServiceBus
        /// </summary>
        /// <param name="topicName">The topic where subscription exists</param>
        /// <param name="subscriptionName">Subscription name</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken);
    }
}
