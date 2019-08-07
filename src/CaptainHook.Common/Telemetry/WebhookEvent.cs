using System.Net;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Common.Telemetry
{
    using System;
    using Eshopworld.Core;

    public class WebhookEvent : TelemetryEvent
    {
        public WebhookEvent()
        {
        }

        public WebhookEvent(Guid handle, string type, string message, string uri, HttpVerb httpVerb, HttpStatusCode statusCode, string correlationId)
        {
            Handle = handle;
            Type = type;
            Uri = uri;
            HttpVerb = httpVerb;
            Message = message;
            StatusCode = statusCode;
            CorrelationId = correlationId;
        }

        public Guid Handle { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }

        public HttpVerb HttpVerb { get; set; }

        public string Message { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string CorrelationId { get; set; }
    }
}