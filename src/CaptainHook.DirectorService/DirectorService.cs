using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Remoting;
using Eshopworld.Core;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CaptainHook.DirectorService
{
    public class DirectorService : StatefulService, IDirectorServiceRemoting
    {
        private readonly IBigBrother _bigBrother;
        private readonly IFabricClientWrapper _fabricClientWrapper;
        private IDictionary<string, SubscriberConfiguration> _subscriberConfigurations;
        private IList<WebhookConfig> _webhookConfigurations;
        

        /// <summary>
        /// Initializes a new instance of <see cref="DirectorService"/>.
        /// </summary>
        /// <param name="context">The injected <see cref="StatefulServiceContext"/>.</param>
        /// <param name="bigBrother">The injected <see cref="IBigBrother"/> telemetry interface.</param>
        /// <param name="fabricClientWrapper">The injected <see cref="IFabricClientWrapper"/>.</param>
        /// <param name="subscriptionManager">The injected <see cref="ISubscriptionManager"/>.</param>
        public DirectorService(
            StatefulServiceContext context,
            IBigBrother bigBrother,
            IFabricClientWrapper fabricClientWrapper, IDictionary<string, SubscriberConfiguration> subscriberConfigurations, IList<WebhookConfig> webhookConfigurations)
            : base(context)
        {
            _bigBrother = bigBrother;
            _fabricClientWrapper = fabricClientWrapper;
            _subscriberConfigurations = subscriberConfigurations;
            _webhookConfigurations = webhookConfigurations;
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
                var serviceList = await _fabricClientWrapper.GetServiceUriListAsync();

                // Handlers:
                if (!serviceList.Contains(ServiceNaming.EventHandlerServiceFullName))
                {
                    var description = new ServiceCreationDescription(ServiceNaming.EventHandlerServiceFullName, ServiceNaming.EventHandlerActorServiceType);
                    await _fabricClientWrapper.CreateServiceAsync(description, cancellationToken);
                }

                var manager = new SubscriptionManager(_fabricClientWrapper, serviceList, _webhookConfigurations);
                await manager.CreateAsync(_subscriberConfigurations, cancellationToken);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception);
                throw;
            }
        }

        public Task ReloadConfigurationForEventAsync(string eventName)
        {
            var configuration = Configuration.Load();
            var comparison = new SubscriberConfigurationComparer().Compare(_subscriberConfigurations, configuration.SubscriberConfigurations);


            _subscriberConfigurations = configuration.SubscriberConfigurations;
            _webhookConfigurations = configuration.WebhookConfigurations;
            return Task.CompletedTask;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
    }
}
