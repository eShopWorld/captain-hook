using System;
using System.Net;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Common.Telemetry
{
    public class FailedWebHookEvent : WebhookEvent
    {
        public FailedWebHookEvent()
        {
        }

        public FailedWebHookEvent(
            string requestHeaders,
            string requestBody, 
            string responseBody, 
            Guid handle, 
            string type, 
            string message, 
            string uri, 
            HttpVerb httpVerb, 
            HttpStatusCode statusCode, 
            string correlationId)
        {
            RequestBody = requestBody;
            ResponseBody = responseBody;
            Handle = handle;
            Type = type;
            Uri = uri;
            HttpVerb = httpVerb;
            Message = message;
            StatusCode = statusCode;
            CorrelationId = correlationId;
        }

        public string RequestHeaders { get; set; }

        public string RequestBody { get; set; }

        public string ResponseBody { get; set; }
    }
}