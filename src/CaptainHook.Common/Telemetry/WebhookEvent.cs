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

        public WebhookEvent(Guid handle, string type, string payload, string state = "success")
        {
            Handle = handle;
            Payload = payload;
            Type = type;
        }

        public Guid Handle { get; set; }

        public string Type { get; set; }

        public string Payload { get; set; }

        public string State { get; set; }
    }

    public class HttpClientFailure : WebhookEvent
    {
        public HttpClientFailure(Guid handle, string type, string payload, string state
        )
            : base(handle, type, payload, state)
        {

        }
    }
}