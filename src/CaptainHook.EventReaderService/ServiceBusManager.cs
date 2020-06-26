using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace CaptainHook.EventReaderService
{
    /// <inheritdoc/>
    public class ServiceBusManager : IServiceBusManager
    {
        private readonly IMessageProviderFactory _factory;

        public ServiceBusManager(IMessageProviderFactory factory)
        {
            _factory = factory;
        }

        public async Task CreateAsync(string azureSubscriptionId, string serviceBusNamespace, string subscriptionName, string topicName, CancellationToken cancellationToken)
        {
            var azureTopic = await ServiceBusNamespaceExtensions.SetupTopic(
                azureSubscriptionId,
                serviceBusNamespace,
                TypeExtensions.GetEntityName(topicName),
                cancellationToken);

            await azureTopic?.CreateSubscriptionIfNotExists(subscriptionName, cancellationToken);
        }

        public IMessageReceiver CreateMessageReceiver(string serviceBusConnectionString, string topicName, string subscriptionName, bool dlqMode)
        {
            return _factory.Create(serviceBusConnectionString, TypeExtensions.GetEntityName(topicName), subscriptionName.ToLowerInvariant(), dlqMode);
        }

        public string GetLockToken(Message message)
        {
            return message.SystemProperties.LockToken;
        }
    }
}
