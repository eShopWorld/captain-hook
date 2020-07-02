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
        public async Task CreateReadersAsync(IEnumerable<SubscriberConfiguration> subscribers, IEnumerable<string> deployedServicesNames, IEnumerable<WebhookConfig> webhooks, CancellationToken cancellationToken)
        {
            var desiredServices = Enumerable.ToDictionary(subscribers, s => ReaderServiceHashSuffixedNameGenerator.GenerateName(s), s => s);

            var servicesToCreate = new Dictionary<string, SubscriberConfiguration>();
            foreach (var (_, subscriber) in desiredServices)
            {
                var name = ReaderServiceHashSuffixedNameGenerator.GenerateName(subscriber);
                if (!deployedServicesNames.Contains(name))
                {
                    servicesToCreate.Add(name, subscriber);
                }
            }

            var servicesToDelete = deployedServicesNames.Except(desiredServices.Keys);

            await CreateReaderServicesAsync(servicesToCreate, webhooks, cancellationToken);
            await DeleteReaderServicesAsync(servicesToDelete, cancellationToken);
        }

        /// <summary>
        /// Compares newly read Configuration with list of currently deployed subscribers and based on that create new, delete old
        /// and refresh (by pair of create and delete operation) existing readers.
        /// </summary>
        /// <param name="newSubscribers">Target subscribers to create</param>
        /// <param name="deployedServicesNames">List of currently deployed services names</param>
        /// <param name="currentSubscribers">List of currently deployed subscribers</param>
        /// <returns></returns>
        public async Task RefreshReadersAsync(IDictionary<string, SubscriberConfiguration> newSubscribers, IEnumerable<WebhookConfig> newWebhooks,
            IDictionary<string, SubscriberConfiguration> currentSubscribers, IEnumerable<string> deployedServicesNames, CancellationToken cancellationToken)
        {
            var desiredSubscriberConfigs = newSubscribers;

            // Prepare service names to compare
            var desiredServices = new Dictionary<string, SubscriberConfiguration>();
            var desiredServiceNames = new List<SubscriberNamesDescription>();

            foreach (var (subscriberName, subscriberConfig) in desiredSubscriberConfigs)
            {
                var serviceName = ServiceNaming.EventReaderServiceFullUri(subscriberConfig.EventType, subscriberConfig.SubscriberName, subscriberConfig.DLQMode.HasValue);
                var suffix = HashCalculator.GetEncodedHash(subscriberConfig);

                var subscriberNamesDescription = new SubscriberNamesDescription(subscriberName, serviceName, suffix);
                desiredServiceNames.Add(subscriberNamesDescription);
                desiredServices.Add(serviceName, subscriberConfig);
            }

            var currentServicesNames = deployedServicesNames.Select(ds => new CurrentServiceNameDescription(ds)).ToList();

            // Detect changes based on passed configuration
            var notChanged = desiredServiceNames
                .Where(desired => currentServicesNames.Any(current => desired.ServiceName == current.ServiceName && desired.FullServiceUri == current.FullServiceUri))
                .ToList();

            var changed = desiredServiceNames
                .Where(desired => currentServicesNames.Any(current => desired.ServiceName == current.ServiceName && desired.FullServiceUri != current.FullServiceUri))
                .ToList();

            var added = desiredServiceNames
                .Where(desired => currentServicesNames.All(current => desired.ServiceName != current.ServiceName))
                .ToList();

            var deleted = currentServicesNames
                .Where(current => desiredServiceNames.All(desired => current.ServiceName != desired.ServiceName))
                .ToList();

            // now we know the numbers, so we can publish event
            // TODO: add more information, like readers url for each event and so
            _bigBrother.Publish(new RefreshSubscribersEvent(added.Select(s => s.Subscriber), deleted.Select(s => s.ServiceName), changed.Select(s => s.Subscriber)));

            // prepare to actual work
            //var allServiceNamesToDelete = deleted.Select(x => x.FullServiceUri).Union(changed.Select(x => x.FullServiceUri));
            var allServiceNamePairsToCreate = added.Union(changed);
            var servicesToCreate = allServiceNamePairsToCreate.ToDictionary(description => description.FullServiceUri, description => desiredServices[description.ServiceName]);

            var allServiceNamesToDelete = currentServicesNames.Select(s => s.FullServiceUri)
                .Except(desiredServiceNames.Select(d => d.FullServiceUri));

            // actual work
            await CreateReaderServicesAsync(servicesToCreate, newWebhooks, cancellationToken);
            await DeleteReaderServicesAsync(allServiceNamesToDelete, cancellationToken);
        }

        private class CurrentServiceNameDescription
        {
            public string ServiceName { get; }
            public string FullServiceUri { get; }

            public CurrentServiceNameDescription(string fullServiceUri)
            {
                ServiceName = RemoveSuffix(fullServiceUri);
                FullServiceUri = fullServiceUri;
            }

            private string RemoveSuffix(string serviceName)
            {
                return Regex.Replace(serviceName, "(|-a|-b|-\\d{14}|-[a-zA-Z0-9]{22})$", string.Empty);
            }
        }

        private class SubscriberNamesDescription
        {
            public string Subscriber { get; }
            public string ServiceName { get; }
            private string Suffix { get; }
            public string FullServiceUri => $"{ServiceName}-{Suffix}";

            public SubscriberNamesDescription(string subscriber, string service, string suffix)
            {
                Subscriber = subscriber;
                ServiceName = service;
                Suffix = suffix;
            }
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
    }
}
