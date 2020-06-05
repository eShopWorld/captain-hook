using Eshopworld.Core;

namespace CaptainHook.DirectorService.Events
{
    public class ReloadConfigFinishedEvent: TimedTelemetryEvent
    {
        public bool IsSuccess { get; set; }
    }
}