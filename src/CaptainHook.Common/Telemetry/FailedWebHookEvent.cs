using System.Net;
using System.Net.Http;
using CaptainHook.Common.Telemetry.Web;

namespace CaptainHook.Common.Telemetry
{
    public class FailedWebHookEvent : WebhookEvent
    {
        public FailedWebHookEvent()
        {
        }

        public FailedWebHookEvent(
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

        public string MessagePayload { get; set; }

        public string RequestBody { get; set; }

        public string ResponseBody { get; set; }

        //only sent for 401 responses
        public string AuthToken { get; set; }
    }
}
