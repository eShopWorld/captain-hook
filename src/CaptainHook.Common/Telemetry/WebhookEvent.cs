namespace CaptainHook.Common.Telemetry
{
    using System;
    using Eshopworld.Core;

    public class WebhookEvent : TelemetryEvent
    {
        public WebhookEvent()
        {
        }

        public WebhookEvent(Guid handle, string type, string message, string uri, string state = "success")
        {
            Handle = handle;
            Type = type;
            State = state;
            Uri = uri;
            Message = message;
        }

        public Guid Handle { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }

        public string Message { get; set; }

        public string State { get; set; }
    }

    public class WebhookErrorEvent : WebhookEvent
    {
        public WebhookErrorEvent(string uri, string message)
        {
            
        }

        public WebhookErrorEvent(Guid handle, string type, string message, string uri, string state = "success") 
            : base(handle, type, message, uri, state)
        {
        }
    }
}