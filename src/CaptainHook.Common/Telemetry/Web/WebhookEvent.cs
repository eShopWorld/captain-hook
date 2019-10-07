using System.Net;
using System.Net.Http;

using Eshopworld.Core;

namespace CaptainHook.Common.Telemetry.Web
{
    public class WebhookEvent : TelemetryEvent
    {
        public WebhookEvent()
        {
        }

        public WebhookEvent(string eventHandlerActorId, string type, string message, string uri, HttpMethod httpMethod, HttpStatusCode statusCode, string correlationId)
        {
            EventHandlerActorId = eventHandlerActorId;
            Type = type;
            Uri = uri;
            HttpMethod = httpMethod;
            Message = message;
            StatusCode = statusCode;
            CorrelationId = correlationId;
        }

        public string EventHandlerActorId { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }

        public HttpMethod HttpMethod { get; set; }

        public string Message { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string CorrelationId { get; set; }

        //only set for failures, likely to be removed in the future (after 401 stabilization and or auth actor introduced)
        public string Token { get; set; }
    }
}
