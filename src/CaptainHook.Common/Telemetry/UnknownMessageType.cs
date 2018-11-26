namespace CaptainHook.Common.Telemetry
{
    using Eshopworld.Core;

    public class UnknownMessageType : TelemetryEvent
    {
        public string Type { get; set; }

        public string Payload { get; set; }
    }
}