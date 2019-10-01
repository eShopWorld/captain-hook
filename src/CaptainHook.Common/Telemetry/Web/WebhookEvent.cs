using System.Net;
using CaptainHook.Common.Configuration;
using Eshopworld.Core;

namespace CaptainHook.Common.Telemetry.Web
{
    public class WebhookEvent : TelemetryEvent
    {
        public WebhookEvent()
        {
        }

        public WebhookEvent(string eventHandlerActorId, string type, string message, string uri, HttpVerb httpVerb, HttpStatusCode statusCode, string correlationId)
        {
            EventHandlerActorId = eventHandlerActorId;
            Type = type;
            Uri = uri;
            HttpVerb = httpVerb;
            Message = message;
            StatusCode = statusCode;
            CorrelationId = correlationId;
        }

        public string EventHandlerActorId { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }

        public HttpVerb HttpVerb { get; set; }

        public string Message { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string CorrelationId { get; set; }

        //TODO: remove
        public string Token { get; set; }
    }
}
