using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.DirectorService.ReaderServiceManagement;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
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
        private CancellationToken _cancellationToken;

        private readonly IBigBrother _bigBrother;
        private readonly IFabricClientWrapper _fabricClientWrapper;
        private readonly IReaderServicesManager _readerServicesManager;
        private readonly IReaderServiceChangesDetector _readerServiceChangeDetector;
        private readonly ISubscriberConfigurationLoader _subscriberConfigurationLoader;
        private readonly ISubscriberEntityToConfigurationMapper _entityToConfigurationMapper;
        private IDictionary<string, SubscriberConfiguration> _subscriberConfigurations;

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
            ISubscriberConfigurationLoader subscriberConfigurationLoader,
            ISubscriberEntityToConfigurationMapper entityToConfigurationMapper)
            : base(context)
        {
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
            _fabricClientWrapper = fabricClientWrapper ?? throw new ArgumentNullException(nameof(fabricClientWrapper));
            _subscriberConfigurationLoader = subscriberConfigurationLoader ?? throw new ArgumentNullException(nameof(subscriberConfigurationLoader));
            _entityToConfigurationMapper = entityToConfigurationMapper;
            _readerServicesManager = readerServicesManager ?? throw new ArgumentNullException(nameof(readerServicesManager));
            _readerServiceChangeDetector = readerServiceChangeDetector ?? throw new ArgumentNullException(nameof(readerServiceChangeDetector));
        }

        private async Task<(IList<WebhookConfig> newWebhookConfig, IDictionary<string, SubscriberConfiguration> newSubscriberConfigurations)> LoadConfigurationAsync()
        {
            var keyVaultUri = Environment.GetEnvironmentVariable(ConfigurationSettings.KeyVaultUriEnvVariable);
            var (webhookConfig, subscriberConfig) = await _subscriberConfigurationLoader.LoadAsync(keyVaultUri);

            var newSubscriberConfigurations = subscriberConfig
                .ToDictionary(x => SubscriberConfiguration.Key(x.EventType, x.SubscriberName));

            return (webhookConfig, newSubscriberConfigurations);
        }

        private async Task LoadConfigurationAndAssignAsync()
        {
            //todo: remove the webhookconfig
            var (newWebhookConfig, newSubscriberConfigurations) = await LoadConfigurationAsync();
            _subscriberConfigurations = newSubscriberConfigurations;
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

                await LoadConfigurationAndAssignAsync();

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

        public Task<IDictionary<string, SubscriberConfiguration>> GetAllSubscribersAsync()
        {
            return Task.FromResult(_subscriberConfigurations);
        }

        private async void ExecuteConfigReload(object state)
        {
            var reloadConfigFinishedTimedEvent = new ReloadConfigFinishedEvent();

            try
            {
                //todo: remove the webhookconfig
                var (newWebhookConfig, newSubscriberConfigurations) = await LoadConfigurationAsync();

                await Task.Delay(60000);

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

        public async Task<CreateReaderResult> CreateReaderAsync(SubscriberConfiguration subscriber)
        {
            if (!_refreshInProgress)
            {
                try
                {
                    Monitor.Enter(_refreshSync);

                    if (!_refreshInProgress)
                    {
                        _refreshInProgress = true;

                        var changeInfo = ReaderChangeInfo.ToBeCreated(new DesiredReaderDefinition(subscriber));

                        return await _readerServicesManager.CreateSingleReaderAsync(changeInfo, _cancellationToken);
                    }
                }
                finally
                {
                    Monitor.Exit(_refreshSync);
                }
            }

            _bigBrother.Publish(new ReloadConfigRequestedWhenAnotherInProgressEvent());
            return CreateReaderResult.DirectorIsBusy;
        }
    }
}
