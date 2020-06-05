namespace CaptainHook.DirectorService.Infrastructure
{
    public class ServiceCreationDescription
    {
        public string ServiceName { get; }
        public string ServiceTypeName { get; }
        public byte[] InitializationData { get; }

        public ServiceCreationDescription(string serviceName, string serviceTypeName, byte[] initializationData = null)
        {
            this.ServiceName = serviceName;
            this.ServiceTypeName = serviceTypeName;
            this.InitializationData = initializationData;
        }
    }
}
