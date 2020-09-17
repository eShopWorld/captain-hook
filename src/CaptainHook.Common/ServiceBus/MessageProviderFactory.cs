using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace CaptainHook.Common.ServiceBus
{
    public class MessageProviderFactory : IMessageProviderFactory
    {
        private ServiceBusConnection conn;
        public IMessageReceiver Create(string serviceBusConnectionString, string topicName, string subscriptionName, bool dlqMode)
        {
            Validate(subscriptionName);
            Validate(serviceBusConnectionString);
            Validate(topicName);

            if (conn == null)
            {
                conn = new ServiceBusConnection(new ServiceBusConnectionStringBuilder(serviceBusConnectionString));
            }

            var path = EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName);

            if (dlqMode)
            {
                path += "/$deadletterqueue";
            }

            return new MessageReceiver(
            conn,
            path,
            ReceiveMode.PeekLock,
            new RetryExponential(
                BackoffMin,
                BackoffMax,
                ReceiverRetryCount),
            ReceiverBatchSize);
        }

        /// <summary>
        /// Batch size for the receiver to consume from the ServiceBus. Defaults to 0
        /// 
        /// NOTE that at the moment this is disabled due to following concerns experienced during runtime
        /// likely in conjuction with the connection reset (to mitigate polling dips) 
        /// messages would normally be pre-fetched and this counts as delivery attempts, when the lock expires, it is again pre-fetched
        /// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-prefetch#if-it-is-faster-why-is-prefetch-not-the-default-option
        /// so at times messages would go to DLQ without being actually processed 
        /// or would get lower than designed-for retry count
        /// </summary>
        public int ReceiverBatchSize { get; set; } = 0; 

        /// <summary>
        /// Minimum backoff time for the ServiceBus retry calls in milliseconds. Defaults to 100ms
        /// </summary>
        public TimeSpan BackoffMin { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Maximum backoff time for the ServiceBus retry calls in milliseconds. Defaults to 500ms
        /// </summary>
        public TimeSpan BackoffMax { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// retry count for inner SB receiver policy
        /// </summary>
        public const int ReceiverRetryCount = 3;

        public IMessageReceiver MessageReceiver { get; private set; }

        private void Validate(string prop)
        {
            if (string.IsNullOrWhiteSpace(prop))
            {
                throw new ArgumentNullException(nameof(prop));
            }
        }

        private void Validate(int prop)
        {
            if (prop == default)
            {
                throw new ArgumentException("value cannot be zero", nameof(prop));
            }
        }
    }
}
