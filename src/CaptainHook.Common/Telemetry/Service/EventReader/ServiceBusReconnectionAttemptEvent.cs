using Eshopworld.Core;

namespace CaptainHook.Common.Telemetry.Service.EventReader
{
    public class ServiceBusReconnectionAttemptEvent : TelemetryEvent
    {
        public int RetryCount { get; set; }
    }
}
