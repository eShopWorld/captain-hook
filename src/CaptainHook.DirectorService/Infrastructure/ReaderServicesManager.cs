using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using Eshopworld.Core;

namespace CaptainHook.DirectorService.Infrastructure
{
    /// <summary>
    /// Allows to create or refresh Reader Services.
    /// </summary>
    public class ReaderServicesManager : IReaderServicesManager
    {
        private readonly IFabricClientWrapper _fabricClientWrapper;
        private readonly IBigBrother _bigBrother;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IReaderServiceNameGenerator _readerServiceNameGenerator;

        /// <summary>
        /// Creates a ReaderServiceManager instance
        /// </summary>
        /// <param name="fabricClientWrapper">Fabric Client</param>
        public ReaderServicesManager(
            IFabricClientWrapper fabricClientWrapper,
            IBigBrother bigBrother,
            IDateTimeProvider dateTimeProvider,
            IReaderServiceNameGenerator readerServiceNameGenerator)
        {
            _fabricClientWrapper = fabricClientWrapper;
            _bigBrother = bigBrother;
            _dateTimeProvider = dateTimeProvider;
            _readerServiceNameGenerator = readerServiceNameGenerator;
        }

        /// <summary>
        /// Creates new instance of readers. Also deletes obsolete and no longer configured ones.
        /// </summary>
        /// <param name="subscribers">List of subscribers to create</param>
        /// <param name="deployedServicesNames">List of currently deployed services names</param>
        /// <param name="webhooks">List of webhook configuration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task CreateReadersAsync(IEnumerable<SubscriberConfiguration> subscribers, 
            IEnumerable<string> deployedServicesNames, 
            IEnumerable<WebhookConfig> webhooks, 
            CancellationToken cancellationToken)
        {
            await DoTheJob(subscribers, webhooks, deployedServicesNames, cancellationToken);
        }

        /// <summary>
        /// Compares newly read Configuration with list of currently deployed subscribers and based on that create new, delete old
        /// and refresh (by pair of create and delete operation) existing readers.
        /// </summary>
        /// <param name="newSubscribers">Target subscribers to create</param>
        /// <param name="deployedServicesNames">List of currently deployed services names</param>
        /// <param name="currentSubscribers">List of currently deployed subscribers</param>
        /// <returns></returns>
        public async Task RefreshReadersAsync(IEnumerable<SubscriberConfiguration> newSubscribers, IEnumerable<WebhookConfig> newWebhooks,
            IDictionary<string, SubscriberConfiguration> currentSubscribers, IEnumerable<string> deployedServicesNames, CancellationToken cancellationToken)
        {
            await DoTheJob(newSubscribers, newWebhooks, deployedServicesNames, cancellationToken);
        }

        private async Task DoTheJob(IEnumerable<SubscriberConfiguration> newSubscribers, 
            IEnumerable<WebhookConfig> newWebhooks, 
            IEnumerable<string> deployedServicesNames,
            CancellationToken cancellationToken)
        {
            // Prepare service descriptions to compare
            var desiredServices = newSubscribers.Select(c => new DesiredSubscriberDefinition(c)).ToList();
            var existingServices = deployedServicesNames.Select(ds => new ExistingServiceDefinition(ds)).ToList();

            // Detect changes
            var changed = desiredServices.Where(d => existingServices.Any(e => d.ServiceName == e.ServiceName && d.FullServiceUri != e.FullServiceUri)).ToList();
            var added = desiredServices.Where(d => existingServices.All(e => d.ServiceName != e.ServiceName)).ToList();
            var deleted = existingServices.Where(e => desiredServices.All(d => e.ServiceName != d.ServiceName)).ToList();

            // now we know the numbers, so we can publish event
            _bigBrother.Publish(new RefreshSubscribersEvent(added.Select(s => s.ServiceName),
                deleted.Select(s => s.ServiceName), changed.Select(s => s.ServiceName)));

            // prepare to actual work
            var servicesToCreate = added.Union(changed).ToDictionary(dsd => dsd.FullServiceUri, kvp => kvp.SubscriberConfig);
            var allServiceNamesToDelete = existingServices.Select(e => e.FullServiceUri)
                .Except(desiredServices.Select(d => d.FullServiceUri));

            // actual work
            await CreateReaderServicesAsync(servicesToCreate, newWebhooks, cancellationToken);
            await DeleteReaderServicesAsync(allServiceNamesToDelete, cancellationToken);
        }

        private async Task CreateReaderServicesAsync(IDictionary<string, SubscriberConfiguration> subscribers, IEnumerable<WebhookConfig> webhooks, CancellationToken cancellationToken)
        {
            foreach (var (name, subscriber) in subscribers)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var initializationData = BuildInitializationData(subscriber, webhooks);
                var description = new ServiceCreationDescription(
                    serviceName: name,
                    serviceTypeName: ServiceNaming.EventReaderServiceType,
                    partitionScheme: new SingletonPartitionSchemeDescription(),
                    initializationData);
                await _fabricClientWrapper.CreateServiceAsync(description, cancellationToken);
                _bigBrother.Publish(new ServiceCreatedEvent(name));
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

        private static byte[] BuildInitializationData(SubscriberConfiguration subscriber, IEnumerable<WebhookConfig> _)
        {
            return EventReaderInitData
                .FromSubscriberConfiguration(subscriber, subscriber)
                .ToByteArray();
        }

        private class ExistingServiceDefinition
        {
            public string ServiceName { get; }
            public string FullServiceUri { get; }

            public ExistingServiceDefinition(string fullServiceUri)
            {
                ServiceName = RemoveSuffix(fullServiceUri);
                FullServiceUri = fullServiceUri;
            }

            private string RemoveSuffix(string serviceName)
            {
                return Regex.Replace(serviceName, "(|-a|-b|-\\d{14}|-[a-zA-Z0-9]{22})$", string.Empty);
            }
        }

        private class DesiredSubscriberDefinition
        {
            public SubscriberConfiguration SubscriberConfig { get; }
            public string ServiceName { get; }
            private string Suffix { get; }
            public string FullServiceUri => $"{ServiceName}-{Suffix}";

            public DesiredSubscriberDefinition(SubscriberConfiguration subscriberConfig)
            {
                SubscriberConfig = subscriberConfig;
                ServiceName = ServiceNaming.EventReaderServiceFullUri(subscriberConfig.EventType, subscriberConfig.SubscriberName, subscriberConfig.DLQMode.HasValue);
                Suffix = HashCalculator.GetEncodedHash(subscriberConfig);
            }
        }
    }
}
