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
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.DirectorService.ReaderServiceManagement;
using Eshopworld.Core;
using JetBrains.Annotations;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ConfigurationSettings = CaptainHook.Common.Configuration.ConfigurationSettings;

namespace CaptainHook.DirectorService
{
    [UsedImplicitly]
    public class DirectorService : StatefulService, IDirectorServiceRemoting
    {
        private volatile bool _refreshInProgress;
        private readonly object _refreshSync = new object();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private CancellationToken _cancellationToken;

        private readonly IBigBrother _bigBrother;
        private readonly IFabricClientWrapper _fabricClientWrapper;
        private readonly IReaderServicesManager _readerServicesManager;
        private readonly IReaderServiceChangesDetector _readerServiceChangeDetector;
        private readonly ISubscriberConfigurationLoader _subscriberConfigurationLoader;
        private IDictionary<string, SubscriberConfiguration> _subscriberConfigurations = new Dictionary<string, SubscriberConfiguration>();

        /// <summary>
        /// Initializes a new instance of <see cref="DirectorService"/>.
        /// </summary>
        /// <param name="context">The injected <see cref="StatefulServiceContext"/>.</param>
        /// <param name="bigBrother">The injected <see cref="IBigBrother"/> telemetry interface.</param>
        /// <param name="readerServicesManager">reader service manager</param>
        /// <param name="readerServiceChangeDetector">reader service change detector</param>
        /// <param name="fabricClientWrapper">The injected <see cref="IFabricClientWrapper"/>.</param>
        public DirectorService(
            StatefulServiceContext context,
            IBigBrother bigBrother,
            IReaderServicesManager readerServicesManager,
            IReaderServiceChangesDetector readerServiceChangeDetector,
            IFabricClientWrapper fabricClientWrapper,
            ISubscriberConfigurationLoader subscriberConfigurationLoader)
            : base(context)
        {
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
            _fabricClientWrapper = fabricClientWrapper ?? throw new ArgumentNullException(nameof(fabricClientWrapper));
            _subscriberConfigurationLoader = subscriberConfigurationLoader ?? throw new ArgumentNullException(nameof(subscriberConfigurationLoader));
            _readerServicesManager = readerServicesManager ?? throw new ArgumentNullException(nameof(readerServicesManager));
            _readerServiceChangeDetector = readerServiceChangeDetector ?? throw new ArgumentNullException(nameof(readerServiceChangeDetector));
        }

        private async Task<IDictionary<string, SubscriberConfiguration>> LoadConfigurationAsync()
        {
            var keyVaultUri = Environment.GetEnvironmentVariable(ConfigurationSettings.KeyVaultUriEnvVariable);
            var subscriberConfig = await _subscriberConfigurationLoader.LoadAsync(keyVaultUri);

            var newSubscriberConfigurations = subscriberConfig
                .ToDictionary(x => SubscriberConfiguration.Key(x.EventType, x.SubscriberName));

            return newSubscriberConfigurations;
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            // TODO: Check fabric node topology - if running below Bronze, set min and target replicas to 1 instead of 3
            try
            {
                lock (_refreshSync)
                {
                    _refreshInProgress = true;
                }

                _subscriberConfigurations = await LoadConfigurationAsync();

                var serviceList = await _fabricClientWrapper.GetServiceUriListAsync();

                // Handlers:
                if (!serviceList.Contains(ServiceNaming.EventHandlerServiceFullName))
                {
                    var description = new ServiceCreationDescription(
                        serviceName: ServiceNaming.EventHandlerServiceFullName,
                        serviceTypeName: ServiceNaming.EventHandlerActorServiceType,
                        partitionScheme: new UniformInt64RangePartitionSchemeDescription(10));

                    await _fabricClientWrapper.CreateServiceAsync(description, cancellationToken);
                }

                var changes = _readerServiceChangeDetector.DetectChanges(_subscriberConfigurations.Values, serviceList);
                await _readerServicesManager.RefreshReadersAsync(changes, cancellationToken);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
                throw;
            }
            finally
            {
                lock (_refreshSync)
                {
                    _refreshInProgress = false;
                }
            }
        }

        public Task<RequestReloadConfigurationResult> RequestReloadConfigurationAsync()
        {
            if (!_refreshInProgress)
            {
                lock (_refreshSync)
                {
                    if (!_refreshInProgress)
                    {
                        _refreshInProgress = true;
                        ThreadPool.QueueUserWorkItem(ExecuteConfigReload);
                        return Task.FromResult(RequestReloadConfigurationResult.ReloadStarted);
                    }
                }
            }

            _bigBrother.Publish(new ReloadConfigRequestedWhenAnotherInProgressEvent());
            return Task.FromResult(RequestReloadConfigurationResult.ReloadInProgress);
        }

        public Task<ReloadConfigurationStatus> GetReloadConfigurationStatusAsync()
        {
            var status = _refreshInProgress ? ReloadConfigurationStatus.InProgress : ReloadConfigurationStatus.Loaded;

            return Task.FromResult(status);
        }

        public Task<IDictionary<string, SubscriberConfiguration>> GetAllSubscribersAsync()
        {
            return Task.FromResult(_subscriberConfigurations);
        }

        private async void ExecuteConfigReload(object state)
        {
            var reloadConfigFinishedTimedEvent = new ReloadConfigFinishedEvent();

            try
            {
                var newSubscriberConfigurations = await LoadConfigurationAsync();
                var deployedServiceNames = await _fabricClientWrapper.GetServiceUriListAsync();

                var changes = _readerServiceChangeDetector.DetectChanges(newSubscriberConfigurations.Values, deployedServiceNames);
                await _readerServicesManager.RefreshReadersAsync(changes, _cancellationToken);

                _subscriberConfigurations = newSubscriberConfigurations;

                reloadConfigFinishedTimedEvent.IsSuccess = true;
            }
            catch
            {
                reloadConfigFinishedTimedEvent.IsSuccess = false;
                throw;
            }
            finally
            {
                _bigBrother.Publish(reloadConfigFinishedTimedEvent);
                lock (_refreshSync)
                {
                    _refreshInProgress = false;
                }
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task<ReaderChangeResult> ApplyReaderChange(ReaderChangeBase readerChange)
        {
            if (!_refreshInProgress)
            {
                await _semaphoreSlim.WaitAsync(_cancellationToken);

                try
                {
                    if (!_refreshInProgress)
                    {
                        _refreshInProgress = true;

                        var deployedServices = await _fabricClientWrapper.GetServiceUriListAsync();
                        var existingReaders = deployedServices
                            .Where(ExistingReaderDefinition.IsValidReaderService)
                            .Select(s => new ExistingReaderDefinition(s));

                        var desiredReader = new DesiredReaderDefinition(readerChange.Subscriber);
                        var existingReader = existingReaders.SingleOrDefault(r => desiredReader.IsTheSameService(r));

                        ReaderChangeInfo changeInfo = default;
                        switch (readerChange)
                        {
                            case CreateReader _:
                                {
                                    if (existingReader.IsValid)
                                        return ReaderChangeResult.ReaderAlreadyExist;

                                    changeInfo = ReaderChangeInfo.ToBeCreated(desiredReader);
                                    break;
                                }
                            case UpdateReader _:
                                {
                                    if (!existingReader.IsValid)
                                    {
                                        return ReaderChangeResult.ReaderDoesNotExist;
                                    }

                                    var noChanges = desiredReader.IsUnchanged(existingReader);
                                    if (noChanges)
                                    {
                                        return ReaderChangeResult.NoChangeNeeded;
                                    }

                                    changeInfo = ReaderChangeInfo.ToBeUpdated(desiredReader, existingReader);
                                    break;
                                }
                        }

                        var refreshResult = await _readerServicesManager.RefreshReadersAsync(new[] { changeInfo }, _cancellationToken);

                        var singleResult = refreshResult.SingleOrDefault();
                        switch (singleResult.Value)
                        {
                            case RefreshReaderResult.Success:
                                {
                                    var key = SubscriberConfiguration.Key(readerChange.Subscriber.EventType, readerChange.Subscriber.SubscriberName);
                                    _subscriberConfigurations[key] = readerChange.Subscriber;
                                    return ReaderChangeResult.Success;
                                }
                            case RefreshReaderResult.CreateFailed:
                                return ReaderChangeResult.CreateFailed;
                            case RefreshReaderResult.DeleteFailed:
                                return ReaderChangeResult.DeleteFailed;
                            default:
                                return ReaderChangeResult.None;
                        }
                    }
                }
                finally
                {
                    _refreshInProgress = false;
                    _semaphoreSlim.Release();
                }
            }

            _bigBrother.Publish(new ReloadConfigRequestedWhenAnotherInProgressEvent());
            return ReaderChangeResult.DirectorIsBusy;
        }
    }
}
