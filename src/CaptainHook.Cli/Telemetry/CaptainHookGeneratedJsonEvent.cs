using Eshopworld.Core;

namespace CaptainHook.Cli.Telemetry
{
    /// <summary>
    /// resx resources converted event
    /// </summary>
    public class CaptainHookGeneratedJsonEvent : TelemetryEvent
    {
        /// <summary>
        /// input resx designation
        /// </summary>
        public string InputFile { get; set; }
    }
}
