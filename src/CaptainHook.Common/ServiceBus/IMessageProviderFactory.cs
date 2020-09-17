using System;
using Microsoft.Azure.ServiceBus.Core;

namespace CaptainHook.Common.ServiceBus
{
    /// <summary>
    /// Wrapper for MessageReceiver so it can be mocked for testing
    /// </summary>
    public interface IMessageProviderFactory
    {
        /// <summary>
        /// Configures the MessageReceiver
        /// </summary>
        /// <param name="serviceBusConnectionString"></param>
        /// <param name="topicName"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="dlqMode">true if meant to receive DLQ messages</param>
        IMessageReceiver Create(string serviceBusConnectionString, string topicName, string subscriptionName, bool dlqMode);

        /// <summary>
        /// Batch size for the receiver to consume from the ServiceBus
        /// </summary>
        int ReceiverBatchSize { get; set; }

        /// <summary>
        /// Minimum backoff time for the ServiceBus retry calls in milliseconds.
        /// </summary>
        TimeSpan BackoffMin { get; set; }

        /// <summary>
        /// Maximum backoff time for the ServiceBus retry calls in milliseconds.
        /// </summary>
        TimeSpan BackoffMax { get; set; }
    }
}
