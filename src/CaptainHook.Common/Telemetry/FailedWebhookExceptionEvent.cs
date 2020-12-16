using System;
using System.Diagnostics.CodeAnalysis;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.Common.Telemetry
{
    [ExcludeFromCodeCoverage]
    public class HttpSendFailureEvent : TelemetryEvent
    {
        public HttpSendFailureEvent(string uri, Exception exception)
        {
            Uri = uri;
            Exception = JsonConvert.SerializeObject(exception, new JsonSerializerSettings()
            {
                ContractResolver = new NoReferencesJsonContractResolver(),
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public string Uri { get; set; }

        public string Exception { get; set; }
    }
}