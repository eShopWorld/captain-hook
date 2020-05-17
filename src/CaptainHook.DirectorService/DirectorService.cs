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
using Eshopworld.Core;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CaptainHook.DirectorService
{
    public class DirectorService : StatefulService
    {
        private readonly IBigBrother _bigBrother;
        private readonly FabricClient _fabricClient;
        private readonly DefaultServiceSettings _defaultServiceSettings;
        private IDictionary<string, SubscriberConfiguration> _subscriberConfigurations { get; }
        private IList<WebhookConfig> _webhookConfigurations { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="DirectorService"/>.
        /// </summary>
        /// <param name="context">The injected <see cref="StatefulServiceContext"/>.</param>
        /// <param name="bigBrother">The injected <see cref="IBigBrother"/> telemetry interface.</param>
        /// <param name="fabricClient">The injected <see cref="FabricClient"/>.</param>
        /// <param name="defaultServiceSettings"></param>
        public DirectorService(
            StatefulServiceContext context,
            IBigBrother bigBrother,
            FabricClient fabricClient,
            DefaultServiceSettings defaultServiceSettings,
            IDictionary<string, SubscriberConfiguration> subscriberConfigurations,
            IList<WebhookConfig> webhookConfigurations)
            : base(context)
        {
            _bigBrother = bigBrother;
            _fabricClient = fabricClient;
            _defaultServiceSettings = defaultServiceSettings;
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

                var serviceList = (await _fabricClient.QueryManager.GetServiceListAsync(new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}")))
                                  .Select(s => s.ServiceName.AbsoluteUri)
                                  .ToList();

                if (!serviceList.Contains(ServiceNaming.EventHandlerServiceFullName))
                {
                    await _fabricClient.ServiceManager.CreateServiceAsync(
                        new StatefulServiceDescription
                        {
                            ApplicationName = new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}"),
                            HasPersistedState = true,
                            MinReplicaSetSize = _defaultServiceSettings.DefaultMinReplicaSetSize,
                            TargetReplicaSetSize = _defaultServiceSettings.DefaultTargetReplicaSetSize,
                            PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription(10),
                            ServiceTypeName = ServiceNaming.EventHandlerActorServiceType,
                            ServiceName = new Uri(ServiceNaming.EventHandlerServiceFullName),
                            PlacementConstraints = _defaultServiceSettings.DefaultPlacementConstraints
                        },
                        TimeSpan.FromSeconds(30),
                        cancellationToken);
                }

                foreach (var (key, subscriber) in _subscriberConfigurations)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    await CreateServiceForSubscriber(subscriber, serviceList, cancellationToken);
                }


                // TODO: Can't do this for internal eshopworld.com|net hosts, otherwise the sharding would be crazy - need to aggregate internal hosts by domain
                //var uniqueHosts = Rules.Select(r => new Uri(r.HookUri).Host).Distinct();
                //var dispatcherServiceList = (await FabricClient.QueryManager.GetServiceListAsync(new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}")))
                //                        .Select(s => s.ServiceName.AbsoluteUri)
                //                        .ToList();

                //todo this might be used for dispatchers per host but that seems a bit drastic
                //foreach (var host in uniqueHosts)
                //{
                //    if (cancellationToken.IsCancellationRequested) return;

                //    var dispatcherServiceNameUri = $"fabric:/{Constants.CaptainHookApplication.ApplicationName}/{Constants.CaptainHookApplication.EventDispatcherServiceName}.{host}";
                //    if (dispatcherServiceList.Contains(dispatcherServiceNameUri)) continue;

                //    await FabricClient.ServiceManager.CreateServiceAsync(
                //        new StatefulServiceDescription
                //        {
                //            ApplicationName = new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}"),
                //            HasPersistedState = true,
                //            DefaultMinReplicaSetSize = 3,
                //            TargetReplicaSetSize = 3,
                //            PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                //            ServiceTypeName = Constants.CaptainHookApplication.EventReaderServiceType,
                //            ServiceName = new Uri(dispatcherServiceNameUri),
                //            InitializationData = Encoding.UTF8.GetBytes(host)
                //        });
                //}
            }
            catch (Exception ex)
            {
                _bigBrother.Publish(ex.ToExceptionEvent());
                throw;
            }
        }

        private async Task CreateServiceForSubscriber(SubscriberConfiguration subscriber, ICollection<string> serviceList, CancellationToken cancellationToken)
        {
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(subscriber.EventType, subscriber.SubscriberName, subscriber.DLQMode != null);

            if (!serviceList.Contains(readerServiceNameUri))
            {
                var initializationData = BuildInitializationData(subscriber);
                await _fabricClient.ServiceManager.CreateServiceAsync(
                    new StatefulServiceDescription
                    {
                        ApplicationName = new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}"),
                        HasPersistedState = true,
                        MinReplicaSetSize = _defaultServiceSettings.DefaultMinReplicaSetSize,
                        TargetReplicaSetSize = _defaultServiceSettings.DefaultTargetReplicaSetSize,
                        PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                        ServiceTypeName = ServiceNaming.EventReaderServiceType,
                        ServiceName = new Uri(readerServiceNameUri),
                        InitializationData = initializationData,
                        PlacementConstraints = _defaultServiceSettings.DefaultPlacementConstraints
                    },
                    TimeSpan.FromSeconds(30),
                    cancellationToken);
            }
        }

        private byte[] BuildInitializationData(SubscriberConfiguration subscriber)
        {
            var webhookConfig = _webhookConfigurations.SingleOrDefault(x => x.Name == subscriber.Name);
            return EventReaderInitData
                .FromSubscriberConfiguration(subscriber, webhookConfig)
                .ToByteArray();
        }
    }
}
