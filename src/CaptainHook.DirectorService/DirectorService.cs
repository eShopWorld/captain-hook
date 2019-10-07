using System;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using Eshopworld.Core;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CaptainHook.DirectorService
{
    public class DirectorService : StatefulService
    {
        private readonly IBigBrother _bigBrother;
        private readonly FabricClient _fabricClient;
        private readonly DefaultServiceSettings _defaultServiceSettings;

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
            DefaultServiceSettings defaultServiceSettings)
            : base(context)
        {
            _bigBrother = bigBrother;
            _fabricClient = fabricClient;
            _defaultServiceSettings = defaultServiceSettings;
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
                //todo this presents a few problems.
                // - we need to ensure rules which are in the db are created
                // - rules which are updated are updated in the handlers and dispatchers
                // - rules which are deleted can only be soft deleted - cosmos change feed does not support hard deletes

                //var iterator = RuleContainer.Items.GetItemIterator<RoutingRule>();
                //while (iterator.HasMoreResults)
                //{
                //    Rules.AddRange(await iterator.FetchNextSetAsync(cancellationToken));
                //}

                //var uniqueEventTypes = Rules.Select(r => r.EventType).Distinct();

                var events = new[]
                {
                    "Core.Events.Test.TrackingDomainEvent",
                    "Checkout.Domain.Infrastructure.DomainEvents.RetailerOrderConfirmationDomainEvent",
                    "Checkout.Domain.Infrastructure.DomainEvents.PlatformOrderCreateDomainEvent",
                    "Nike.Snkrs.Core.Events.ProductRefreshEvent",
                    "Nike.Snkrs.Core.Events.ProductUpdatedEvent",
                    "Nike.Snkrs.ControlTowerApi.Models.Events.NikeLaunchDataReceivedEvent",
                    "Bullfrog.DomainEvents.ScaleChange",
                    "Eshopworld.Platform.Events.Logistics.ReturnOrderEvent"
                };

                var serviceList = (await _fabricClient.QueryManager.GetServiceListAsync(new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}")))
                                  .Select(s => s.ServiceName.AbsoluteUri)
                                  .ToList();

                if (!serviceList.Contains(Constants.CaptainHookApplication.Services.EventHandlerServiceFullName))
                {
                    await _fabricClient.ServiceManager.CreateServiceAsync(
                        new StatefulServiceDescription
                        {
                            ApplicationName = new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}"),
                            HasPersistedState = true,
                            MinReplicaSetSize = _defaultServiceSettings.DefaultMinReplicaSetSize,
                            TargetReplicaSetSize = _defaultServiceSettings.DefaultTargetReplicaSetSize,
                            PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription(10),
                            ServiceTypeName = Constants.CaptainHookApplication.Services.EventHandlerActorServiceType,
                            ServiceName = new Uri(Constants.CaptainHookApplication.Services.EventHandlerServiceFullName),
                            PlacementConstraints = _defaultServiceSettings.DefaultPlacementConstraints
                        },
                        TimeSpan.FromSeconds(30),
                        cancellationToken);
                }

                foreach (var type in events)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    var readerServiceNameUri = $"{Constants.CaptainHookApplication.Services.EventReaderServiceFullName}.{type}";
                    if (!serviceList.Contains(readerServiceNameUri))
                    {
                        await _fabricClient.ServiceManager.CreateServiceAsync(
                            new StatefulServiceDescription
                            {
                                ApplicationName = new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}"),
                                HasPersistedState = true,
                                MinReplicaSetSize = _defaultServiceSettings.DefaultMinReplicaSetSize,
                                TargetReplicaSetSize = _defaultServiceSettings.DefaultTargetReplicaSetSize,
                                PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                                ServiceTypeName = Constants.CaptainHookApplication.Services.EventReaderServiceType,
                                ServiceName = new Uri(readerServiceNameUri),
                                InitializationData = Encoding.UTF8.GetBytes(type),
                                PlacementConstraints = _defaultServiceSettings.DefaultPlacementConstraints
                            }, 
                            TimeSpan.FromSeconds(30), 
                            cancellationToken );
                    }
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
    }
}
