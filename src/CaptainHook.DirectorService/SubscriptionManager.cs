using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;

namespace CaptainHook.DirectorService
{
    /// <summary>
    /// Allows to create, delete and refresh Reader Services instances.
    /// </summary>
    public class ReaderServicesManager
    {
        private readonly IFabricClientWrapper _fabricClientWrapper;
        private readonly IList<WebhookConfig> _webhookConfigurations;
        private readonly IList<string> _serviceList;

        /// <summary>
        /// Creates a ReaderServiceManager instance
        /// </summary>
        /// <param name="fabricClientWrapper">Fabric Client</param>
        /// <param name="serviceList">List of currently deployed services.</param>
        /// <param name="webhookConfigurations">Recent list of webhook configurations</param>
        public ReaderServicesManager(IFabricClientWrapper fabricClientWrapper, IList<string> serviceList, IList<WebhookConfig> webhookConfigurations)
        {
            _fabricClientWrapper = fabricClientWrapper;
            _serviceList = serviceList;
            _webhookConfigurations = webhookConfigurations;
        }

        public async Task CreateAsync(IEnumerable<SubscriberConfiguration> subscribers, CancellationToken cancellationToken)
        {
            foreach (var subscriber in subscribers)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var (newName, oldNames) = FindServiceNames(subscriber);
                var initializationData = BuildInitializationData(subscriber);

                var description = new ServiceCreationDescription(newName, ServiceNaming.EventReaderServiceType, initializationData);

                await _fabricClientWrapper.CreateServiceAsync(description, cancellationToken);
            }
        }

        public async Task DeleteAsync(IEnumerable<SubscriberConfiguration> subscribers, CancellationToken cancellationToken)
        {
            foreach (var subscriber in subscribers)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var (newName, oldNames) = FindServiceNames(subscriber);

                foreach (var oldName in oldNames.Where(n => n != null))
                {
                    await _fabricClientWrapper.DeleteServiceAsync(oldName, cancellationToken);
                }
            }
        }

        public async Task RefreshAsync(IEnumerable<SubscriberConfiguration> subscribers, CancellationToken cancellationToken)
        {
            foreach (var subscriber in subscribers)
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
