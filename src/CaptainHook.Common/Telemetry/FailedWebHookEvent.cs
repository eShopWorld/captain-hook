using System.Net;
using System.Net.Http;
using CaptainHook.Common.Telemetry.Web;

namespace CaptainHook.Common.Telemetry
{
    public class FailedWebhookEvent : WebhookEvent
    {
        public FailedWebhookEvent()
        {
        }

        public FailedWebhookEvent(
            string requestHeaders,
            string responseHeaders,
            string requestBody, 
            string responseBody, 
            string messagePayload,
            string eventHandlerActorId,
            string type, 
            string message, 
            string uri, 
            HttpMethod httpMethod, 
            HttpStatusCode statusCode, 
            string correlationId,
            string webhookRules) 
            : base(eventHandlerActorId, type, message, uri, httpMethod, statusCode, correlationId, webhookRules)
        {
            RequestHeaders = requestHeaders;
            ResponseHeaders = responseHeaders;
            RequestBody = requestBody;
            ResponseBody = responseBody;
            MessagePayload = messagePayload;
        }

        public string RequestHeaders { get; set; }

        public string ResponseHeaders { get; set; }

        /// <remarks>
        /// This is sent for all environments except for SAND and PROD
        /// </remarks>
        public string MessagePayload { get; set; }

        /// <remarks>
        /// This is sent for all environments except for SAND and PROD
        /// </remarks>
        public string RequestBody { get; set; }

        public string ResponseBody { get; set; }

        /// <remarks>
        /// This is only sent for 401 responses
        /// </remarks>
        public string AuthToken { get; set; }
    }
}
