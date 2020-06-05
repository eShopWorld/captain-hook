using System.Fabric.Description;

namespace CaptainHook.DirectorService.Infrastructure
{
    

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
