using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService
{
    /// <summary>
    /// A simple wrapper around FabricClient
    /// </summary>
    public class FabricClientWrapper : IFabricClientWrapper
    {
        private readonly FabricClient fabricClient;
        private readonly DefaultServiceSettings defaultServiceSettings;

        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Initializes a new instance of this class
        /// </summary>
        /// <param name="fabricClient"></param>
        public FabricClientWrapper(FabricClient fabricClient, DefaultServiceSettings defaultServiceSettings)
        {
            this.fabricClient = fabricClient;
            this.defaultServiceSettings = defaultServiceSettings;
        }

        public async Task<List<string>> GetServiceUriListAsync()
        {
            var uri = new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}");

            var serviceList = await fabricClient.QueryManager.GetServiceListAsync(uri);

            return serviceList.Select(s => s.ServiceName.AbsoluteUri).ToList();
        }

        public async Task CreateServiceAsync(ServiceCreationDescription serviceCreationDescription, CancellationToken cancellationToken)
        {
            var serviceDescription = new StatefulServiceDescription
            {
                ApplicationName = new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}"),
                HasPersistedState = true,
                MinReplicaSetSize = defaultServiceSettings.DefaultMinReplicaSetSize,
                TargetReplicaSetSize = defaultServiceSettings.DefaultTargetReplicaSetSize,
                PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription(10),
                ServiceTypeName = serviceCreationDescription.ServiceTypeName,
                ServiceName = new Uri(serviceCreationDescription.ServiceName),
                InitializationData = serviceCreationDescription.InitializationData,
                PlacementConstraints = defaultServiceSettings.DefaultPlacementConstraints
            };

            await fabricClient.ServiceManager.CreateServiceAsync(serviceDescription, timeout, cancellationToken);
        }

        public async Task DeleteServiceAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            var uri = new Uri(serviceName);
            var serviceDescription = new DeleteServiceDescription(uri);

            await fabricClient.ServiceManager.DeleteServiceAsync(serviceDescription, timeout, cancellationToken);
        }
    }
}
