namespace CaptainHook.Common.Telemetry
{
    using System;

    public class HttpClientFailure : WebhookEvent
    {
        public HttpClientFailure(Guid handle, string type, string payload, string state
        )
            : base(handle, type, payload, state)
        {

        }
    }
}