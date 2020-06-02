using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService
{
    public class SubscriptionManager
    {
        private readonly IFabricClientWrapper fabricClientWrapper;
        private readonly IList<WebhookConfig> webhookConfigurations;
        private readonly IList<string> serviceList;

        public SubscriptionManager(IFabricClientWrapper fabricClientWrapper, IList<string> serviceList, IList<WebhookConfig> webhookConfigurations)
        {
            this.fabricClientWrapper = fabricClientWrapper;
            this.serviceList = serviceList;
            this.webhookConfigurations = webhookConfigurations;
        }

        public async Task CreateAsync(IDictionary<string, SubscriberConfiguration> subscriberConfigurations, CancellationToken cancellationToken)
        {
            foreach (var (_, subscriber) in subscriberConfigurations)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var (newName, oldNames) = FindServiceNames(subscriber);
                var initializationData = BuildInitializationData(subscriber);

                var description = new ServiceCreationDescription(newName, ServiceNaming.EventReaderServiceType, initializationData);

                await this.fabricClientWrapper.CreateServiceAsync(description, cancellationToken);
            }
        }

        private (string newName, IEnumerable<string> oldNames) FindServiceNames(SubscriberConfiguration subscriber)
        {
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(subscriber.EventType, subscriber.SubscriberName, subscriber.DLQMode != null);

            var names = new[] { readerServiceNameUri, $"{readerServiceNameUri}-a", $"{readerServiceNameUri}-b" };

            var oldNames = serviceList.Intersect(names);

            var newName = $"{readerServiceNameUri}-a";
            if (oldNames.Contains(newName))
            {
                newName = $"{readerServiceNameUri}-b";
            }

            return (newName, oldNames);
        }

        private byte[] BuildInitializationData(SubscriberConfiguration subscriber)
        {
            var webhookConfig = webhookConfigurations.SingleOrDefault(x => x.Name == subscriber.Name);
            return EventReaderInitData
                .FromSubscriberConfiguration(subscriber, webhookConfig)
                .ToByteArray();
        }
    }
}
