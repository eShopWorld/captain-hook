using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Base62;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.Infrastructure
{
    /// <summary>
    /// Allows to create or refresh Reader Services.
    /// </summary>
    public class ReaderServicesManager : IReaderServicesManager
    {
        readonly struct ReaderTask
        {
            public readonly string Name;
            public readonly Task Task;

            public ReaderTask (string name, Task task)
            {
                Name = name;
                Task = task;
            }

        }

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
        /// Compares newly read Configuration with list of currently deployed subscribers and based on that create new, delete old
        /// and refresh (by pair of create and delete operation) existing readers.
        /// </summary>
        /// <param name="newSubscribers">Target subscribers to create</param>
        /// <param name="deployedServicesNames">List of currently deployed services names</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RefreshReadersAsync(IEnumerable<SubscriberConfiguration> newSubscribers, IEnumerable<string> deployedServicesNames, CancellationToken cancellationToken)
        {
            // Describe the situation
            var desiredReaders = newSubscribers.Select(c => new DesiredReaderDefinition(c)).ToList();
            var existingReaders = deployedServicesNames
                .Where(s => s.StartsWith($"fabric:/{Constants.CaptainHookApplication.ApplicationName}/{ServiceNaming.EventReaderServiceShortName}", StringComparison.InvariantCultureIgnoreCase))
                .Select(s => new ExistingReaderDefinition(s)).ToList();

            // Detect changes
            var changed = desiredReaders.Where(d => existingReaders.Any(e => d.ServiceName == e.ServiceName && d.ServiceNameWithSuffix != e.ServiceNameWithSuffix)).ToList();
            var added = desiredReaders.Where(d => existingReaders.All(e => d.ServiceName != e.ServiceName)).ToList();
            var deleted = existingReaders.Where(e => desiredReaders.All(d => e.ServiceName != d.ServiceName)).ToList();

            // now we know the numbers, so we can publish event
            _bigBrother.Publish(new RefreshSubscribersEvent(added.Select(s => s.ServiceName), deleted.Select(s => s.ServiceName), changed.Select(s => s.ServiceName)));

            // prepare to actual work (put changed to added, delete everything except desired)
            var servicesToCreate = added.Union(changed).ToDictionary(dsd => dsd.ServiceNameWithSuffix, kvp => kvp.SubscriberConfig);
            var allServiceNamesToDelete = existingReaders.Select(e => e.ServiceNameWithSuffix).Except(desiredReaders.Select(d => d.ServiceNameWithSuffix));

            // actual work
            await CreateReaderServicesAsync(servicesToCreate, cancellationToken);
            await DeleteReaderServicesAsync(allServiceNamesToDelete, cancellationToken);
        }

        private async Task CreateReaderServicesAsync(IDictionary<string, SubscriberConfiguration> subscribers, CancellationToken cancellationToken)
        {
            foreach (var (name, subscriber) in subscribers)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var initializationData = EventReaderInitData.FromSubscriberConfiguration(subscriber, subscriber).ToByteArray();

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
            var toDelete = oldNames.ToHashSet ();

            while (toDelete.Any ())
            {

                var tasks = toDelete
                    .Select (name => new ReaderTask(name, _fabricClientWrapper.DeleteServiceAsync (name, cancellationToken)))
                    .ToArray ();

                var deletionEvent = new ReaderServicesDeletionEvent ();

                try
                {
                    await Task.WhenAll (tasks.Select (t => t.Task));
                    toDelete.Clear ();

                    deletionEvent.SetDeletedNames (tasks.Select (t => t.Name));
                    _bigBrother.Publish (deletionEvent);
                }
                catch (Exception)
                {
                    var completedTasks = tasks
                        .Where (t => t.Task.Exception == null || t.Task.Exception.InnerExceptions.OfType<FabricServiceNotFoundException> ().Any ())
                        .ToArray ();

                    var failedTasks = tasks.Except (completedTasks);

                    deletionEvent.SetDeletedNames (completedTasks.Select (t => t.Name));
                    deletionEvent.SetFailed (failedTasks.Select (t => new FailedReaderServiceDeletion (t.Name, t.Task.Exception.Message)));

                    _bigBrother.Publish (deletionEvent);

                    toDelete.ExceptWith (completedTasks.Select (t => t.Name));
                }
            }
        }

        private class ExistingReaderDefinition
        {
            private static readonly Regex RemoveSuffixRegex = new Regex("(|-a|-b|-\\d{14}|-[a-zA-Z0-9]{22})$", RegexOptions.Compiled);

            public string ServiceName { get; }
            public string ServiceNameWithSuffix { get; }

            public ExistingReaderDefinition(string serviceNameWithSuffix)
            {
                ServiceName = RemoveSuffixRegex.Replace(serviceNameWithSuffix, string.Empty);
                ServiceNameWithSuffix = serviceNameWithSuffix;
            }
        }

        private class DesiredReaderDefinition
        {
            public SubscriberConfiguration SubscriberConfig { get; }
            public string ServiceName { get; }
            public string ServiceNameWithSuffix { get; }

            public DesiredReaderDefinition(SubscriberConfiguration subscriberConfig)
            {
                SubscriberConfig = subscriberConfig;
                ServiceName = ServiceNaming.EventReaderServiceFullUri(subscriberConfig.EventType, subscriberConfig.SubscriberName, subscriberConfig.DLQMode.HasValue);
                ServiceNameWithSuffix = $"{ServiceName}-{GetEncodedHash(subscriberConfig)}";
            }

            private static string GetEncodedHash(SubscriberConfiguration configuration)
            {
                using (var md5 = new MD5CryptoServiceProvider())
                {
                    var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(configuration));
                    var hash = md5.ComputeHash(bytes);
                    var encoded = hash.ToBase62();
                    return encoded;
                }
            }
        }
    }
}
