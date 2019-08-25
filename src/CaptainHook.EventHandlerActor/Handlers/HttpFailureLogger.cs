using System;
using System.Net;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// Wrapper for logging http failures in CH which include data re the endpoint being hit and the event from which the request came
    /// </summary>
    [Obsolete("Need to wrap the http handlers in a polly rather than wrap the retries in a http client")]
    public class HttpFailureLogger
    {
        private readonly IBigBrother _bigBrother;
        private readonly MessageData _message;
        private readonly string _uri;
        private readonly HttpVerb _httpVerb;

        public HttpFailureLogger(IBigBrother bigBrother, MessageData message, string uri, HttpVerb httpVerb)
        {
            _bigBrother = bigBrother;
            _message = message;
            _uri = uri;
            _httpVerb = httpVerb;
        }

        public void Publish(string message, HttpStatusCode statusCode, string correlationId)
        {
            _bigBrother.Publish(new HttpClientFailure(_message.Handle, _message.Type, message, _uri, _httpVerb, statusCode, correlationId));
        }
    }
}