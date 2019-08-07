namespace CaptainHook.Common.Telemetry
{
    using System;
    using Eshopworld.Core;

    public class WebhookEvent : TelemetryEvent
    {
        public WebhookEvent()
        {
        }

        public WebhookEvent(Guid handle, string type, string message, string uri)
        {
            Handle = handle;
            Type = type;
            Uri = uri;
            Message = message;
        }

        public Guid Handle { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }

        public string Message { get; set; }
    }
}