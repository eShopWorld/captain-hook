namespace CaptainHook.Common.Telemetry
{
    using System;
    using Eshopworld.Core;

    public class WebhookEvent : TelemetryEvent
    {
        public WebhookEvent(string payload, string state = "failure")
        {
            this.Payload = payload;
            this.State = state;
        }

        public WebhookEvent(Guid handle, string type, string payload, string uri, string state = "success")
        {
            Handle = handle;
            Payload = payload;
            Type = type;
            State = state;
            Uri = uri;
        }

        public Guid Handle { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }

        public string Payload { get; set; }

        public string State { get; set; }
    }
}