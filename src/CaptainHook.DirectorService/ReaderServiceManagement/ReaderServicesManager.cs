using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using Eshopworld.Core;

namespace CaptainHook.DirectorService.ReaderServiceManagement
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

            public ReaderTask(string name, Task task)
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

        public async Task RefreshReadersAsync(IEnumerable<ReaderChangeInfo> changeSet, CancellationToken cancellationToken)
        {
            LogEvent(changeSet.ToArray());

            var servicesToCreate = changeSet
                .Where(change => (change.ChangeType & ReaderChangeType.ToBeCreated) != 0)
                .Select(change => change.NewReader);

            var allServiceNamesToDelete = changeSet
                .Where(change => (change.ChangeType & ReaderChangeType.ToBeRemoved) != 0)
                .Select(change => change.OldReader.ServiceNameWithSuffix);

            await CreateReaderServicesAsync(servicesToCreate, cancellationToken);
            await DeleteReaderServicesAsync(allServiceNamesToDelete, cancellationToken);
        }

        public async Task<ReaderProvisionResult> ProvisionReaderAsync(SubscriberConfiguration subscriber, CancellationToken cancellationToken)
        {
            var result = ReaderProvisionResult.None;

            var deployedServices = await _fabricClientWrapper.GetServiceUriListAsync();
            var existingReaders = ExistingReadersBuilder.FromNames(deployedServices);
            var desiredReader = new DesiredReaderDefinition(subscriber);
            var existingReader = existingReaders.SingleOrDefault(r => desiredReader.IsTheSameService(r));
            var changeInfo = CreateChangeInfo(existingReader, desiredReader);

            if (existingReader.IsValid)
            {
                result |= ReaderProvisionResult.ReaderAlreadyExists;
            }

            LogEvent(changeInfo);

            if (changeInfo.ChangeType.HasFlag(ReaderChangeType.ToBeCreated))
            {
                var creationResult = await CreateReaderServicesAsync(new List<DesiredReaderDefinition> { changeInfo.NewReader }, cancellationToken);
                result |= creationResult ? ReaderProvisionResult.Created : ReaderProvisionResult.CreateFailed;
            }

            if (changeInfo.ChangeType.HasFlag(ReaderChangeType.ToBeRemoved) && !result.HasFlag(ReaderProvisionResult.CreateFailed))
            {
                var deletionResult = await DeleteReaderServicesAsync(new[] { changeInfo.OldReader.ServiceNameWithSuffix }, cancellationToken);
                result |= deletionResult ? ReaderProvisionResult.Deleted : ReaderProvisionResult.CreateFailed;
            }

            return result;
        }

        private static ReaderChangeInfo CreateChangeInfo(ExistingReaderDefinition existingReader, DesiredReaderDefinition desiredReader)
        {
            if (!existingReader.IsValid)
            {
                return ReaderChangeInfo.ToBeCreated(desiredReader);
            }

            if (!desiredReader.IsUnchanged(existingReader))
            {
                return ReaderChangeInfo.ToBeUpdated(desiredReader, existingReader);
            }

            return new ReaderChangeInfo();
        }

        private void LogEvent(params ReaderChangeInfo[] changeSet)
        {
            var added = changeSet
                .Where(change => change.ChangeType == ReaderChangeType.ToBeCreated)
                .Select(change => change.NewReader.ServiceName);

            var deleted = changeSet
                .Where(change => change.ChangeType == ReaderChangeType.ToBeRemoved)
                .Select(change => change.OldReader.ServiceName);

            var updated = changeSet
                .Where(change => change.ChangeType == ReaderChangeType.ToBeUpdated)
                .Select(change => change.NewReader.ServiceName);

            _bigBrother.Publish(new RefreshSubscribersEvent(added, deleted, updated));
        }

        private async Task<bool> CreateReaderServicesAsync(IEnumerable<DesiredReaderDefinition> readerDefinitions, CancellationToken cancellationToken)
        {
            foreach (var reader in readerDefinitions)
            {
                if (cancellationToken.IsCancellationRequested) return false;

                var initializationData = EventReaderInitData.FromSubscriberConfiguration(reader.SubscriberConfig).ToByteArray();

                var description = new ServiceCreationDescription(
                    serviceName: reader.ServiceNameWithSuffix,
                    serviceTypeName: ServiceNaming.EventReaderServiceType,
                    partitionScheme: new SingletonPartitionSchemeDescription(),
                    initializationData);
                try
                {
                    await _fabricClientWrapper.CreateServiceAsync(description, cancellationToken);
                    _bigBrother.Publish(new ReaderServiceCreatedEvent(reader.ServiceNameWithSuffix));
                }
                catch (Exception e)
                {
                    _bigBrother.Publish(new ReaderServiceCreationFailedEvent(reader.ServiceNameWithSuffix, e));
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> DeleteReaderServicesAsync(IEnumerable<string> oldNames, CancellationToken cancellationToken)
        {
            var toDelete = oldNames.ToHashSet();

            while (toDelete.Any())
            {

                var tasks = toDelete
                    .Select(name => new ReaderTask(name, _fabricClientWrapper.DeleteServiceAsync(name, cancellationToken)))
                    .ToArray();

                var deletionEvent = new ReaderServicesDeletionEvent();

                try
                {
                    await Task.WhenAll(tasks.Select(t => t.Task));
                    toDelete.Clear();

                    deletionEvent.SetDeletedNames(tasks.Select(t => t.Name));
                    _bigBrother.Publish(deletionEvent);
                }
                catch (Exception)
                {
                    var completedTasks = tasks
                        .Where(t => t.Task.Exception == null || t.Task.Exception.InnerExceptions.OfType<FabricServiceNotFoundException>().Any())
                        .ToArray();

                    var failedTasks = tasks.Except(completedTasks);

                    deletionEvent.SetDeletedNames(completedTasks.Select(t => t.Name));
                    deletionEvent.SetFailed(failedTasks.Select(t => new FailedReaderServiceDeletion(t.Name, t.Task.Exception.Message)));

                    _bigBrother.Publish(deletionEvent);

                    toDelete.ExceptWith(completedTasks.Select(t => t.Name));
                    return false;
                }
            }

            return true;
        }
    }
}
