namespace CaptainHook.Common.Telemetry
{
    using System;
    using Eshopworld.Core;

    public class WebhookExecuted : TelemetryEvent
    {
        public WebhookExecuted()
        {
            
        }

        public WebhookExecuted(Guid handle, string type, string payload)
        {
            Handle = handle;
            Payload = payload;
            Type = type;
        }

        public Guid Handle { get; set; }

        public string Type { get; set; }

        public string Payload { get; set; }
    }
}