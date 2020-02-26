namespace CaptainHook.Common
{
    public struct Constants
    {
        public struct Headers
        {
            public const string CorrelationId = "X-Correlation-ID";

            public const string ContentType = "Content-Type";

            public const string EventType = "Esw-Event-Type";

            public const string EventDeliveryId = "Esw-Delivery";

            public const string Authorization = "Authorization";

            public const string DefaultContentType = "application/json";

            public const string IdempotencyKey = "Idempotency-Key";
        }

        public struct CaptainHookApplication
        {
            public const string ApplicationName = "CaptainHook";

            public struct DefaultServiceConfig
            {
                public const string DefaultMinReplicaSetSize = "DefaultMinReplicaSetSize";

                public const string TargetReplicaSetSize = "DefaultTargetReplicaSetSize";

                public const string DefaultPartitionCount = "DefaultPartitionCount";

                public const string DefaultPlacementConstraints = "DefaultPlacementConstraints";
            }
        }
    }
}
