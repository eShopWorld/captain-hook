using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Utils;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService
{
    /// <summary>
    /// Allows to create or refresh Reader Services.
    /// </summary>
    public class ReaderServicesManager : IReaderServicesManager
    {
        private readonly IFabricClientWrapper _fabricClientWrapper;
        private readonly IBigBrother _bigBrother;

        /// <summary>
        /// Creates a ReaderServiceManager instance
        /// </summary>
        /// <param name="fabricClientWrapper">Fabric Client</param>
        public ReaderServicesManager(IFabricClientWrapper fabricClientWrapper, IBigBrother bigBrother)
        {
            _fabricClientWrapper = fabricClientWrapper;
            _bigBrother = bigBrother;
        }

        /// <summary>
        /// Creates new instance of readers. Also deletes obsolete and no longer configured ones.
        /// </summary>
        /// <param name="subscribers">List of subscribers to create</param>
        /// <param name="serviceList">List of currently deployed services names</param>
        /// <param name="webhooks">List of webhook configuration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task CreateReadersAsync(IEnumerable<SubscriberConfiguration> subscribers, IList<string> serviceList, IList<WebhookConfig> webhooks, CancellationToken cancellationToken)
        {
            // we must delete previous instances also as they may have obsolete configuration
            var servicesToCreate = subscribers.ToDictionary(s => FindServiceNames(s, serviceList).newName, s => s);
            var servicesToDelete = subscribers.SelectMany(s => FindServiceNames(s, serviceList).oldNames);

            await CreateReaderServicesAsync(servicesToCreate, webhooks, cancellationToken);
            await DeleteReaderServicesAsync(servicesToDelete, cancellationToken);
        }

        /// <summary>
        /// Compares newly read Configuration with list of currently deployed subscribers and based on that create new, delete old
        /// and refresh (by pair of create and delete operation) existing readers.
        /// </summary>
        /// <param name="newConfiguration">Target Configuration to be deployed</param>
        /// <param name="serviceList">List of currently deployed services names</param>
        /// <param name="currentSubscribers">List of currently deployed subscribers</param>
        /// <returns></returns>
        public async Task RefreshReadersAsync(Configuration newConfiguration, IDictionary<string, SubscriberConfiguration> currentSubscribers, IList<string> serviceList, CancellationToken cancellationToken)
        {
            var comparisonResult = new SubscriberConfigurationComparer().Compare(currentSubscribers, newConfiguration.SubscriberConfigurations);
            _bigBrother.Publish(new RefreshSubscribersEvent(comparisonResult));

            var servicesToCreate = comparisonResult.Added.Values.Union(comparisonResult.Changed.Values).ToDictionary(s => FindServiceNames(s, serviceList).newName, s => s);
            var servicesToDelete = comparisonResult.Removed.Values.Union(comparisonResult.Changed.Values).SelectMany(s => FindServiceNames(s, serviceList).oldNames);

            await CreateReaderServicesAsync(servicesToCreate, newConfiguration.WebhookConfigurations, cancellationToken);
            await DeleteReaderServicesAsync(servicesToDelete, cancellationToken);
        }

        private async Task CreateReaderServicesAsync(IDictionary<string, SubscriberConfiguration> subscribers, IList<WebhookConfig> webhooks, CancellationToken cancellationToken)
        {
            foreach (var (name, subscriber) in subscribers)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var initializationData = BuildInitializationData(subscriber, webhooks);
                var description = new ServiceCreationDescription(name, ServiceNaming.EventReaderServiceType, initializationData);
                await _fabricClientWrapper.CreateServiceAsync(description, cancellationToken);
                _bigBrother.Publish(new ServiceCreatedEvent(name, JsonConvert.SerializeObject(subscriber)));
            }
        }

        private async Task DeleteReaderServicesAsync(IEnumerable<string> oldNames, CancellationToken cancellationToken)
        {
            foreach (var oldName in oldNames)
            {
                if (cancellationToken.IsCancellationRequested) return;

                await _fabricClientWrapper.DeleteServiceAsync(oldName, cancellationToken);
                _bigBrother.Publish(new ServiceDeletedEvent(oldName));
            }
        }

        private static (string newName, IEnumerable<string> oldNames) FindServiceNames(SubscriberConfiguration subscriber, IList<string> serviceList)
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

        private static byte[] BuildInitializationData(SubscriberConfiguration subscriber, IEnumerable<WebhookConfig> webhooks)
        {
            var webhookConfig = webhooks.SingleOrDefault(x => x.Name == subscriber.Name);
            return EventReaderInitData
                .FromSubscriberConfiguration(subscriber, webhookConfig)
                .ToByteArray();
        }
    }
}
