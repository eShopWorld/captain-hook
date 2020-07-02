using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Extensions;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using Eshopworld.Core;
using Kusto.Cloud.Platform.Utils;
using Newtonsoft.Json;

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
            var namesGenerator = new ReaderServiceHashSuffixedNameGenerator();

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
            //var comparisonResult = new SubscriberConfigurationComparer().Compare(currentSubscribers, newSubscribers);
            //_bigBrother.Publish(new RefreshSubscribersEvent(comparisonResult));

            //var servicesToCreate = comparisonResult.Added.Values.Union(comparisonResult.Changed.Values).ToDictionary(s => _readerServiceNameGenerator.GenerateNewName(s.ToSubscriberNaming()), s => s);
            //var servicesToDelete = comparisonResult.Removed.Values.Union(comparisonResult.Changed.Values).SelectMany(s => _readerServiceNameGenerator.FindOldNames(s.ToSubscriberNaming(), deployedServicesNames));

            //var subscribers = newSubscribers.Values;

            // find hash-suffixed names for subscribers 
            //var currentNamesForSubscribersMap = newSubscribers.ToDictionary(x => x.Key, x => namesGenerator.GenerateName(x.Value));
            // we already have deployed readers with these names, so these won't be regenerated
            //var subscribersNotChanged = currentNamesForSubscribersMap.Where(x => deployedServicesNames.Contains(x.Value));
            // we'd like to identify completely new readers
            // totallyNewSubscribers = 

            var desiredSubscriberConfigs = newSubscribers;

            // Prepare service names to compare
            var desiredServices = new Dictionary<string, SubscriberConfiguration>();
            var desiredServiceNames = new List<SubscriberNamesDescription>();

            foreach (var (subscriberName, subscriberConfig) in desiredSubscriberConfigs)
            {
                var serviceName = ServiceNaming.EventReaderServiceFullUri(subscriberConfig.EventType, subscriberConfig.SubscriberName, subscriberConfig.DLQMode.HasValue);
                var suffix = HashCalculator.GetEncodedHash(subscriberConfig);
                //var serviceName = ReaderServiceHashSuffixedNameGenerator.GenerateName(subscriberConfig);

                var subscriberNamesDescription = new SubscriberNamesDescription(subscriberName, serviceName, suffix);
                desiredServiceNames.Add(subscriberNamesDescription);
                desiredServices.Add(serviceName, subscriberConfig);
            }

            var currentServicesNames = deployedServicesNames.Select(ds => new CurrentServiceNameDescription(RemoveSuffix(ds), ds)).ToList();

            //  first.Where(x => second.Count(y => comparer(x, y)) == 0);

            // Calculate changes
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

            //var notChanged = desiredServiceNames.Intersect(currentServicesNames, (a, b) => a.Subscriber == b.Subscriber && a.Service == b.Service).ToList();
            //var changed = desiredServiceNames.Intersect(currentServicesNames, (a, b) => a.Subscriber == b.Subscriber && a.Service != b.Service).ToList();
            //var added = desiredServiceNames.Except(currentServicesNames, (a, b) => a.Subscriber == b.Subscriber).ToList();
            //var deleted = currentServicesNames.Except(desiredServiceNames, (a, b) => a.Subscriber == b.Subscriber).ToList();

            // now we know the numbers, so we can publish event
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

            //desiredServices = subscribers.ToDictionary(s => namesGenerator.GenerateName(s), s => s);

            //var desiredServicesNames = newSubscribers.Select(x => x.Key)


            //foreach (var (subscriberName, subscriberConfig) in desiredServices)
            //{
            //    // if we can match desired subscriber name with existing subscriber name
            //    // and reader services names also match, then don't touch (add to list?)
            //    // but if reader services names doesn't match then need to be changed
            //    // so add to "event changed" list and add to "to create" and "to delete" list

            //    if (existingServices.TryGetValue(subscriberName, out string existingName))
            //    {


            //    }



            //    // if desired names doesn't contain existing service name
            //    // add to "event deleted" list and add to "to delete" list

            //    // if desired name can't be found in existing names
            //    // add to "event added" list and add to "to create" list


            //    //var serviceName = namesGenerator.GenerateName(subscriberConfig);
            //    //if (!deployedServicesNames.Contains(serviceName))
            //    //{
            //    //    servicesToCreate.Add(serviceName, subscriberConfig);
            //    //}
            //}

            //var servicesToCreate = new Dictionary<string, SubscriberConfiguration>();
            //var servicesToDelete = new Dictionary<string, SubscriberConfiguration>();


            //var addedServices = desiredServices.Keys.Except(deployedServicesNames);
            //var deletedServices = deployedServicesNames.Except(desiredServices.Keys);
            //var changedServices = desiredServices.Keys.Intersect(deployedServicesNames);

            //_bigBrother.Publish(new RefreshSubscribersEvent(addedServices, deletedServices, changedServices));


        }

        class CurrentServiceNameDescription
        {
            public string ServiceName { get; }
            public string FullServiceUri { get; }

            public CurrentServiceNameDescription(string serviceName, string fullServiceUri)
            {
                ServiceName = serviceName;
                FullServiceUri = fullServiceUri;
            }
        }


        private class SubscriberNamesDescription
        {
            public string Subscriber { get; }
            public string ServiceName { get; }
            public string Suffix { get; }
            public string FullServiceUri => $"{ServiceName}-{Suffix}";

            public SubscriberNamesDescription(string subscriber, string service, string suffix)
            {
                Subscriber = subscriber;
                ServiceName = service;
                Suffix = suffix;
            }
        }

        private string RemoveSuffix(string serviceName)
        {
            //string eventSubscriberAndSufix = serviceName.Replace($"fabric:/{Constants.CaptainHookApplication.ApplicationName}/{ServiceNaming.EventReaderServiceShortName}.", string.Empty);
            string subscriberName = Regex.Replace(serviceName, "(|-a|-b|-\\d{14}|-[a-zA-Z0-9]{22})$", string.Empty);
            return subscriberName;
        }

        //private ConfigurationComparision Compare(IDictionary<string, SubscriberConfiguration> newSubscribers, IEnumerable<string> deployedServicesNames)
        //{
        //    var subscribers = newSubscribers.Values;

        //    var namesGenerator = new ReaderServiceHashSuffixedNameGenerator();

        //    // find hash-suffixed names for subscribers 
        //    var currentNamesForSubscribersMap = newSubscribers.ToDictionary(x => x.Key, x => namesGenerator.GenerateName(x.Value));
        //    // we already have deployed readers with these names, so these won't be regenerated
        //    var subscribersNotChanged = currentNamesForSubscribersMap.Where(x => deployedServicesNames.Contains(x.Value));
        //    // we'd like to identify completely new readers
        //    // totallyNewSubscribers = 

        //    var desiredServices = subscribers.ToDictionary(s => namesGenerator.GenerateName(s), s => s);

        //    var servicesToCreate = new Dictionary<string, SubscriberConfiguration>();
        //    foreach (var (_, subscriber) in desiredServices)
        //    {
        //        var name = namesGenerator.GenerateName(subscriber);
        //        if (!deployedServicesNames.Contains(name))
        //        {
        //            servicesToCreate.Add(name, subscriber);
        //        }
        //    }

        //    var servicesToDelete = deployedServicesNames.Except(desiredServices.Keys);

        //    var addedServices = desiredServices.Keys.Except(deployedServicesNames);
        //    var deletedServices = deployedServicesNames.Except(desiredServices.Keys);
        //    var changedServices = desiredServices.Keys.Intersect(deployedServicesNames);

        //    return new ConfigurationComparision(addedServices);
        //}

        private class ConfigurationComparision
        {
            public Dictionary<string, string> AddedSubscribers { get; }
            public Dictionary<string, string> ChangedSubscribers { get; }
            public Dictionary<string, string> DeletedSubscribers { get; }
            public Dictionary<string, string> NotTouchedSubscribers { get; }

            public ConfigurationComparision(Dictionary<string, string> addedSubscribers,
                Dictionary<string, string> changedSubscribers,
                Dictionary<string, string> deletedSubscribers,
                Dictionary<string, string> notTouchedSubscribers)
            {
                AddedSubscribers = addedSubscribers;
                ChangedSubscribers = changedSubscribers;
                DeletedSubscribers = deletedSubscribers;
                NotTouchedSubscribers = notTouchedSubscribers;
            }
        }

        class MyClass
        {
            public Dictionary<string, string> ReadersToBeCreated { get; private set; }
            public Dictionary<string, string> ReadersToBeDeleted { get; private set; }
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

    public static class LinqExtensions
    {
        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TSource, bool> comparer)
        {
            return first.Where(a => second.Count(b => comparer(a, b)) == 0);
        }

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TSource, bool> comparer)
        {
            return first.Where(a => second.Count(b => comparer(a, b)) == 1);
        }
    }
}
