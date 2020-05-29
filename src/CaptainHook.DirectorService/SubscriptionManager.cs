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
        private IDictionary<string, SubscriberConfiguration> subscriberConfigurations;
        private IList<WebhookConfig> webhookConfigurations;

        public SubscriptionManager(
            IFabricClientWrapper fabricClientWrapper,
            IDictionary<string, SubscriberConfiguration> subscriberConfigurations,
            IList<WebhookConfig> webhookConfigurations)
        {
            this.fabricClientWrapper = fabricClientWrapper;
            this.subscriberConfigurations = subscriberConfigurations;
            this.webhookConfigurations = webhookConfigurations;
        }

        private static (string newName, IEnumerable<string> oldNames) FindServiceNames(ICollection<string> serviceList, string readerServiceNameUri)
        {
            var names = new[] { readerServiceNameUri, $"{readerServiceNameUri}-a", $"{readerServiceNameUri}-b" };

            var oldNames = serviceList.Intersect(names);

            var newName = $"{readerServiceNameUri}-a";
            if (oldNames.Contains(newName))
            {
                newName = $"{readerServiceNameUri}-b";
            }

            return (newName, oldNames);
        }

        public async Task CreateServicesAsync(CancellationToken cancellationToken)
        {
            var serviceList = await fabricClientWrapper.GetServiceUriListAsync();

            if (!serviceList.Contains(ServiceNaming.EventHandlerServiceFullName))
            {
                await fabricClientWrapper.CreateServiceAsync(ServiceNaming.EventHandlerServiceFullName, cancellationToken);
            }

            foreach (var (_, subscriber) in subscriberConfigurations)
            {
                if (cancellationToken.IsCancellationRequested) return;

                await RecreateServiceForSubscriberAsync(subscriber, serviceList, cancellationToken);
            }
        }

        private byte[] BuildInitializationData(SubscriberConfiguration subscriber)
        {
            var webhookConfig = webhookConfigurations.SingleOrDefault(x => x.Name == subscriber.Name);
            return EventReaderInitData
                .FromSubscriberConfiguration(subscriber, webhookConfig)
                .ToByteArray();
        }

        private async Task RecreateServiceForSubscriberAsync(SubscriberConfiguration subscriber, ICollection<string> serviceList, CancellationToken cancellationToken)
        {
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(subscriber.EventType, subscriber.SubscriberName, subscriber.DLQMode != null);

            var (newName, oldNames) = FindServiceNames(serviceList, readerServiceNameUri);

            var initializationData = BuildInitializationData(subscriber);

            await fabricClientWrapper.CreateServiceAsync(newName, cancellationToken);

            foreach (var oldName in oldNames.Where(n => n != null))
            {
                await fabricClientWrapper.DeleteServiceAsync(oldName, cancellationToken);
            }
        }

    }
}
