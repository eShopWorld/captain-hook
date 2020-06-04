using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Remoting;
using CaptainHook.DirectorService.Utils;
using Eshopworld.Core;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CaptainHook.DirectorService
{
    public class DirectorService : StatefulService, IDirectorServiceRemoting
    {
        private bool _refreshInProgress;
        private readonly object _refreshSync = new object();

        private readonly IBigBrother _bigBrother;
        private readonly IFabricClientWrapper _fabricClientWrapper;
        private readonly IReaderServicesManager _readerServicesManager;
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
            IDictionary<string, SubscriberConfiguration> subscriberConfigurations,
            IList<WebhookConfig> webhookConfigurations)
            : base(context)
        {
            _bigBrother = bigBrother;
            _fabricClientWrapper = fabricClientWrapper;
            _subscriberConfigurations = subscriberConfigurations;
            _webhookConfigurations = webhookConfigurations;
            _readerServicesManager = readerServicesManager;
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Check fabric node topology - if running below Bronze, set min and target replicas to 1 instead of 3
            try
            {
                lock (_refreshSync)
                {
                    _refreshInProgress = true;
                }

                var serviceList = await _fabricClientWrapper.GetServiceUriListAsync();

                // Handlers:
                if (!serviceList.Contains(ServiceNaming.EventHandlerServiceFullName))
                {
                    var description = new ServiceCreationDescription(ServiceNaming.EventHandlerServiceFullName, ServiceNaming.EventHandlerActorServiceType);
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

        public async Task ReloadConfigurationAsync()
        {
            lock (_refreshSync)
            {
                if (_refreshInProgress)
                    return;

                _refreshInProgress = true;
            }

            try
            {
                var configuration = Configuration.Load();
                var serviceList = await _fabricClientWrapper.GetServiceUriListAsync();

                await _readerServicesManager.RefreshReadersAsync(configuration, _subscriberConfigurations, serviceList, CancellationToken.None);

                _subscriberConfigurations = configuration.SubscriberConfigurations;
                _webhookConfigurations = configuration.WebhookConfigurations;
            }
            finally
            {
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
