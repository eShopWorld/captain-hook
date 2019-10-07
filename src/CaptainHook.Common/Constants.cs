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

            public struct Services
            {
                public const string EventReaderServiceShortName = "EventReader";

                public static readonly string EventReaderServiceFullName = $"fabric:/{ApplicationName}/{EventReaderServiceShortName}";

                public const string EventReaderServiceType = "CaptainHook.EventReaderServiceType";

                public const string DirectorServiceShortName = "Director";

                public const string DirectorServiceType = "CaptainHook.DirectorServiceType";

                public const string EventDispatcherServiceName = "Dispatcher";

                public const string EventDispatcherServiceType = "CaptainHook.EventDispatcherServiceType";

                public const string EventHandlerServiceShortName = "EventHandler";

                public static readonly string EventHandlerServiceFullName = $"fabric:/{ApplicationName}/{EventHandlerServiceShortName}";

                public const string EventHandlerActorServiceType = "EventHandlerActorServiceType";

            }
        }
    }
}