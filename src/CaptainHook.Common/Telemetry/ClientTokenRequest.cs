using Eshopworld.Core;

namespace CaptainHook.Common.Telemetry
{
    public class ClientTokenRequest : TelemetryEvent
    {
        public string ClientId { get; set; }
        public string Authority { get; set; }
    }
}