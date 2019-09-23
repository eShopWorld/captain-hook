using System;
using System.Fabric;

namespace CaptainHook.Common.Telemetry.Service
{
    public abstract class ServiceException : Exception
    {
        protected ServiceException(string message, StatefulServiceContext context) : base(message)
        {
            ServiceName = context.ServiceName.AbsoluteUri;
            ServiceType = context.ServiceTypeName;
            ReplicaId = context.ReplicaId;
            PartitionId = context.PartitionId;
        }

        public string ServiceName { get; set; }

        public string ServiceType { get; set; }

        public Guid PartitionId { get; set; }

        public long ReplicaId { get; set; }
    }
}