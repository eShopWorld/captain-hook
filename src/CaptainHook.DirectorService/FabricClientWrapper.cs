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
        private readonly FabricClient _fabricClient;
        private readonly DefaultServiceSettings _defaultServiceSettings;

        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Initializes a new instance of this class
        /// </summary>
        /// <param name="fabricClient"></param>
        public FabricClientWrapper(FabricClient fabricClient, DefaultServiceSettings defaultServiceSettings)
        {
            _fabricClient = fabricClient;
            _defaultServiceSettings = defaultServiceSettings;
        }

        public async Task<List<string>> GetServiceUriListAsync()
        {
            var uri = new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}");

            var serviceList = await _fabricClient.QueryManager.GetServiceListAsync(uri);

            return serviceList.Select(s => s.ServiceName.AbsoluteUri).ToList();
        }

        public async Task CreateServiceAsync(ServiceCreationDescription serviceCreationDescription, CancellationToken cancellationToken)
        {
            var serviceDescription = new StatefulServiceDescription
            {
                ApplicationName = new Uri($"fabric:/{Constants.CaptainHookApplication.ApplicationName}"),
                HasPersistedState = true,
                MinReplicaSetSize = _defaultServiceSettings.DefaultMinReplicaSetSize,
                TargetReplicaSetSize = _defaultServiceSettings.DefaultTargetReplicaSetSize,
                PartitionSchemeDescription = serviceCreationDescription.PartitionScheme,
                ServiceTypeName = serviceCreationDescription.ServiceTypeName,
                ServiceName = new Uri(serviceCreationDescription.ServiceName),
                InitializationData = serviceCreationDescription.InitializationData,
                PlacementConstraints = _defaultServiceSettings.DefaultPlacementConstraints
            };

            await _fabricClient.ServiceManager.CreateServiceAsync(serviceDescription, timeout, cancellationToken);
        }

        public async Task DeleteServiceAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            var uri = new Uri(serviceName);
            var serviceDescription = new DeleteServiceDescription(uri);

            await _fabricClient.ServiceManager.DeleteServiceAsync(serviceDescription, timeout, cancellationToken);
        }
    }
}
