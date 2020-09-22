using System;
using System.Fabric;
using Eshopworld.Core;

namespace CaptainHook.Common.Telemetry.Service
{
    public abstract class ServiceTelemetryEvent : TelemetryEvent
    {
        protected ServiceTelemetryEvent(StatefulServiceContext context)
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