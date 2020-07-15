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

        public WebhookEvent(string eventHandlerActorId, string type, string message, string uri, HttpMethod httpMethod, HttpStatusCode statusCode, string correlationId, string webhookRules)
        {
            EventHandlerActorId = eventHandlerActorId;
            Type = type;
            Uri = uri;
            HttpMethod = httpMethod;
            Message = message;
            StatusCode = statusCode;
            CorrelationId = correlationId;
            WebhookRules = webhookRules;
        }

        public string EventHandlerActorId { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }

        public HttpMethod HttpMethod { get; set; }

        public string Message { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string CorrelationId { get; set; }

        /// <summary>
        /// Without routes information, just source/dest rules
        /// </summary>
        public string WebhookRules { get; set; }
    }
}