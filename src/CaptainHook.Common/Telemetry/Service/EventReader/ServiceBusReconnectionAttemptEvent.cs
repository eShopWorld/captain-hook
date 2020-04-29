using Eshopworld.Core;

namespace CaptainHook.Common.Telemetry.Service.EventReader
{
    public class ServiceBusReconnectionAttemptEvent : TelemetryEvent
    {
        public int RetryCount { get; set; }
        public string EventType { get; set; }
        public string SubscriptionName { get; set; }
    }
}
