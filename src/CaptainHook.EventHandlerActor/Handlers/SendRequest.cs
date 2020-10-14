using System;
using System.Net.Http;
using JetBrains.Annotations;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class SendRequest
    {
        public SendRequest(
            HttpMethod httpMethod,
            Uri uri,
            WebHookHeaders headers,
            string payload,
            [NotNull]TimeSpan[] retrySleepDurations,
            TimeSpan timeout = default)
        {
            HttpMethod = httpMethod;
            Uri = uri;
            Headers = headers;
            Payload = payload;
            Timeout = timeout;
            RetrySleepDurations = retrySleepDurations ?? throw new ArgumentNullException(nameof(retrySleepDurations), "Retry sleep durations are required");
        }

        public HttpMethod HttpMethod { get; }

        public Uri Uri { get; }

        public WebHookHeaders Headers { get; }

        public string Payload { get; }

        public TimeSpan Timeout { get; }

        public TimeSpan[] RetrySleepDurations { get; }
    }
}