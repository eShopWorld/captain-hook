using System.Diagnostics.CodeAnalysis;
using Eshopworld.Core;

namespace CaptainHook.Common.Telemetry.Web
{
    [ExcludeFromCodeCoverage]
    public class AuthenticationEvent : TelemetryEvent
    {
        public string Message { get; set; }
    }
}
