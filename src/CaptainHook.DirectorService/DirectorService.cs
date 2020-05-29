using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Remoting;
using CaptainHook.Common.ServiceModels;
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
        /// <param name="fabricClientWrapper">The injected <see cref="IFabricClientWrapper"/> fabric client wrapper interface.</param>
        public DirectorService(
            StatefulServiceContext context,
            IBigBrother bigBrother,
            IFabricClientWrapper fabricClientWrapper,
            IDictionary<string, SubscriberConfiguration> subscriberConfigurations,
            IList<WebhookConfig> webhookConfigurations)
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

                if (!serviceList.Contains(ServiceNaming.EventHandlerServiceFullName))
                {
                    await _fabricClientWrapper.CreateServiceAsync(ServiceNaming.EventHandlerServiceFullName, cancellationToken);
                }

                foreach (var (key, subscriber) in _subscriberConfigurations)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    await RecreateServiceForSubscriberAsync(subscriber, serviceList, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception);
                throw;
            }
        }

        private async Task RecreateServiceForSubscriberAsync(SubscriberConfiguration subscriber, ICollection<string> serviceList, CancellationToken cancellationToken)
        {
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(subscriber.EventType, subscriber.SubscriberName, subscriber.DLQMode != null);

            var (newName, oldNames) = FindServiceNames(serviceList, readerServiceNameUri);

            var initializationData = BuildInitializationData(subscriber);
            
            await _fabricClientWrapper.CreateServiceAsync(newName, cancellationToken);

            foreach (var oldName in oldNames.Where(n => n != null))
            {
                await _fabricClientWrapper.DeleteServiceAsync(oldName, cancellationToken);
            }

        }

        private static (string newName, IEnumerable<string> oldNames) FindServiceNames(ICollection<string> serviceList, string readerServiceNameUri)
        {
            var names = new[] { readerServiceNameUri, $"{readerServiceNameUri}-a", $"{readerServiceNameUri}-b" };

            var oldNames = serviceList.Intersect(names);

            var newName = $"{readerServiceNameUri}-a";
            if (oldNames.Contains(newName))
            {
                newName = $"{readerServiceNameUri}-b";
            }

            return (newName, oldNames);
        }

        private byte[] BuildInitializationData(SubscriberConfiguration subscriber)
        {
            var webhookConfig = _webhookConfigurations.SingleOrDefault(x => x.Name == subscriber.Name);
            return EventReaderInitData
                .FromSubscriberConfiguration(subscriber, webhookConfig)
                .ToByteArray();
        }

        public Task ReloadConfigurationForEventAsync(string eventName)
        {
            var configuration = Configuration.Load();
            _subscriberConfigurations = configuration.SubscriberConfigurations;
            return Task.CompletedTask;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
    }
}
