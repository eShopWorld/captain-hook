using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace CaptainHook.EventReaderService
{
    /// <summary>
    /// A wrapper for ServiceBus Functions
    /// </summary>
    public interface IServiceBusManager
    {
        /// <summary>
        /// Creates a topic based on the specified type and subscription to that topic with the specify type
        /// </summary>
        /// <param name="azureSubscriptionId"></param>
        /// <param name="serviceBusNamespace"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="topicName"></param>
        /// <returns></returns>
        Task CreateAsync(string azureSubscriptionId, string serviceBusNamespace, string subscriptionName, string topicName);

        /// <summary>
        /// Creates a message receiver to the service bus namespace
        /// </summary>
        /// <param name="serviceBusConnectionString"></param>
        /// <param name="topicName"></param>
        /// <param name="subscriptionName"></param>
        /// <returns></returns>
        IMessageReceiver CreateMessageReceiver(string serviceBusConnectionString, string topicName, string subscriptionName);

        /// <summary>
        /// Abstraction around the ServiceBusMessage to get lock token
        /// MS have this pretty locked down so there isn't much we can do with mocking or reflection on the Message itself.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string GetLockToken(Message message);
    }
}