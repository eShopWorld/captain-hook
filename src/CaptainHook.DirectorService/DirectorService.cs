using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Remoting;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using Eshopworld.Core;
using JetBrains.Annotations;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

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
        private readonly ISubscriberConfigurationLoader _subscriberConfigurationLoader;
        private IDictionary<string, SubscriberConfiguration> _subscriberConfigurations;
        private IList<WebhookConfig> _webhookConfigurations;

        /// <summary>
        /// Initializes a new instance of <see cref="DirectorService"/>.
        /// </summary>
        /// <param name="context">The injected <see cref="StatefulServiceContext"/>.</param>
        /// <param name="bigBrother">The injected <see cref="IBigBrother"/> telemetry interface.</param>
        /// <param name="fabricClientWrapper">The injected <see cref="IFabricClientWrapper"/>.</param>
        public DirectorService(
            StatefulServiceContext context,
            IBigBrother bigBrother,
            IReaderServicesManager readerServicesManager,
            IFabricClientWrapper fabricClientWrapper,
            ISubscriberConfigurationLoader subscriberConfigurationLoader)
            : base(context)
        {
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
            _fabricClientWrapper = fabricClientWrapper ?? throw new ArgumentNullException(nameof(fabricClientWrapper));
            _subscriberConfigurationLoader = subscriberConfigurationLoader ?? throw new ArgumentNullException(nameof(subscriberConfigurationLoader));
            _readerServicesManager = readerServicesManager ?? throw new ArgumentNullException(nameof(readerServicesManager));
        }

        private async Task<(IList<WebhookConfig> newWebhookConfig, IDictionary<string, SubscriberConfiguration> newSubscriberConfigurations)> LoadConfigurationAsync()
        {
            var (webhookConfig, subscriberConfig) = await _subscriberConfigurationLoader.LoadAsync();

            var newSubscriberConfigurations = subscriberConfig
                .ToDictionary(x => SubscriberConfiguration.Key(x.EventType, x.SubscriberName));

            return (webhookConfig, newSubscriberConfigurations);
        }

        private async Task LoadConfigurationAndAssignAsync()
        {
            var (newWebhookConfig, newSubscriberConfigurations) = await LoadConfigurationAsync();
            _webhookConfigurations = newWebhookConfig;
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

                await _readerServicesManager.CreateReadersAsync(_subscriberConfigurations.Values, serviceList, _webhookConfigurations, cancellationToken);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception);
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
                var (newWebhookConfig, newSubscriberConfigurations) = await LoadConfigurationAsync();

                var deployedServiceNames = await _fabricClientWrapper.GetServiceUriListAsync();

                await _readerServicesManager.RefreshReadersAsync(
                    newSubscriberConfigurations.Values,
                    newWebhookConfig,
                    _subscriberConfigurations,
                    deployedServiceNames,
                    _cancellationToken);

                _webhookConfigurations = newWebhookConfig;
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
    }
}
