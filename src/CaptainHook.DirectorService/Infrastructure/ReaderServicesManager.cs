using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Extensions;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using Eshopworld.Core;

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
            // we must delete previous instances also as they may have obsolete configuration
            var servicesToCreate = subscribers.ToDictionary(s => _readerServiceNameGenerator.GenerateNewName(s.ToSubscriberNaming()), s => s);
            var servicesToDelete = subscribers.SelectMany(s => _readerServiceNameGenerator.FindOldNames(s.ToSubscriberNaming(), deployedServicesNames));

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
        public async Task RefreshReadersAsync(IDictionary<string, SubscriberConfiguration> newSubscribers, IEnumerable<WebhookConfig> newWebhooks, IDictionary<string, SubscriberConfiguration> currentSubscribers, IEnumerable<string> deployedServicesNames, CancellationToken cancellationToken)
        {
            var comparisonResult = new SubscriberConfigurationComparer().Compare(currentSubscribers, newSubscribers);
            _bigBrother.Publish(new RefreshSubscribersEvent(comparisonResult));

            var servicesToCreate = comparisonResult.Added.Values.Union(comparisonResult.Changed.Values).ToDictionary(s => _readerServiceNameGenerator.GenerateNewName(s.ToSubscriberNaming()), s => s);
            var servicesToDelete = comparisonResult.Removed.Values.Union(comparisonResult.Changed.Values).SelectMany(s => _readerServiceNameGenerator.FindOldNames(s.ToSubscriberNaming(), deployedServicesNames));

            await CreateReaderServicesAsync(servicesToCreate, newWebhooks, cancellationToken);
            await DeleteReaderServicesAsync(servicesToDelete, cancellationToken);
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

        private static byte[] BuildInitializationData(SubscriberConfiguration subscriber, IEnumerable<WebhookConfig> _)
        {
            return EventReaderInitData
                .FromSubscriberConfiguration(subscriber, subscriber)
                .ToByteArray();
        }
    }
}
