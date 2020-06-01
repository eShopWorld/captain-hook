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
        private readonly IFabricClientWrapper _fabricClientWrapper;
        private readonly IList<WebhookConfig> _webhookConfigurations;
        private readonly IList<string> _serviceList;

        public SubscriptionManager(IFabricClientWrapper fabricClientWrapper, IList<string> serviceList, IList<WebhookConfig> webhookConfigurations)
        {
            _fabricClientWrapper = fabricClientWrapper;
            _serviceList = serviceList;
            _webhookConfigurations = webhookConfigurations;
        }

        public async Task CreateAsync(IReadOnlyDictionary<string, SubscriberConfiguration> subscriberConfigurations, CancellationToken cancellationToken)
        {
            foreach (var (_, subscriber) in subscriberConfigurations)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var (newName, oldNames) = FindServiceNames(subscriber);
                var initializationData = BuildInitializationData(subscriber);

                var description = new ServiceCreationDescription(newName, ServiceNaming.EventReaderServiceType, initializationData);

                await _fabricClientWrapper.CreateServiceAsync(description, cancellationToken);
            }
        }

        public async Task DeleteAsync(IReadOnlyDictionary<string, SubscriberConfiguration> subscriberConfigurations, CancellationToken cancellationToken)
        {
            foreach (var (_, subscriber) in subscriberConfigurations)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var (newName, oldNames) = FindServiceNames(subscriber);

                foreach (var oldName in oldNames.Where(n => n != null))
                {
                    await _fabricClientWrapper.DeleteServiceAsync(oldName, cancellationToken);
                }
            }
        }

        public async Task RefreshAsync(IReadOnlyDictionary<string, SubscriberConfiguration> subscriberConfigurations, CancellationToken cancellationToken)
        {
            foreach (var (_, subscriber) in subscriberConfigurations)
            {
                var (newName, oldNames) = FindServiceNames(subscriber);
                var initializationData = BuildInitializationData(subscriber);
                var description = new ServiceCreationDescription(newName, ServiceNaming.EventReaderServiceType, initializationData);

                await _fabricClientWrapper.CreateServiceAsync(description, cancellationToken);
                foreach (var oldName in oldNames.Where(n => n != null))
                {
                    await _fabricClientWrapper.DeleteServiceAsync(oldName, cancellationToken);
                }
            }
        }

        private (string newName, IEnumerable<string> oldNames) FindServiceNames(SubscriberConfiguration subscriber)
        {
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(subscriber.EventType, subscriber.SubscriberName, subscriber.DLQMode != null);

            var names = new[] { readerServiceNameUri, $"{readerServiceNameUri}-a", $"{readerServiceNameUri}-b" };

            var oldNames = _serviceList.Intersect(names);

            var newName = $"{readerServiceNameUri}-a";
            if (oldNames.Contains(newName))
            {
                newName = $"{readerServiceNameUri}-b";
            }

            return (newName, oldNames);
        }

        private byte[] BuildInitializationData(SubscriberConfiguration subscriber)
        {
            var webhookConfig = _webhookConfigurations.SingleOrDefault(x => x.Name == subscriber.Name);
            return EventReaderInitData
                .FromSubscriberConfiguration(subscriber, webhookConfig)
                .ToByteArray();
        }
    }
}
