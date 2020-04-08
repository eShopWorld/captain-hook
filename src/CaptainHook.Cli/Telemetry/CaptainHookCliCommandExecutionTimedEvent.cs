using Eshopworld.Core;

namespace CaptainHook.Cli.Telemetry
{
    /// <summary>
    /// timed event for any command executed
    /// </summary>
    public sealed class CaptainHookCliCommandExecutionTimedEvent : TimedTelemetryEvent
    {
        /// <summary>
        /// name of command that produced the exception
        /// </summary>
        public string CommandType { get; set; }

        /// <summary>
        /// arguments passed to the command
        /// </summary>
        public string Arguments { get; set; }

    }
}
