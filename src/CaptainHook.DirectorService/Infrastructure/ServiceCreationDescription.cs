﻿using System.Collections.Generic;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService.Utils
{
    public interface IFabricClientWrapper
    {
        Task<List<string>> GetServiceUriListAsync();

        Task CreateServiceAsync(ServiceCreationDescription serviceCreationDescription, CancellationToken cancellationToken);

        Task DeleteServiceAsync(string serviceName, CancellationToken cancellationToken);
    }

    public class ServiceCreationDescription
    {
        public string ServiceName { get; }
        public string ServiceTypeName { get; }
        public byte[] InitializationData { get; }
        public PartitionSchemeDescription PartitionScheme { get; }

        public ServiceCreationDescription(string serviceName, string serviceTypeName, PartitionSchemeDescription partitionScheme, byte[] initializationData = null)
        {
            this.ServiceName = serviceName;
            this.ServiceTypeName = serviceTypeName;
            this.InitializationData = initializationData;
            this.PartitionScheme = partitionScheme;
        }
    }
}
