using System.Net;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Common.Telemetry
{
    using System;

    public class HttpClientFailure : WebhookEvent
    {
        public HttpClientFailure(Guid handle, string type, string message, string uri, HttpVerb httpVerb, HttpStatusCode statusCode, string correlationId)
            : base(handle, type, message, uri, httpVerb, statusCode, correlationId)
        {

        }
    }
}