using Eshopworld.Core;

namespace CaptainHook.EventReaderService.Events
{
    public class GracefulShutdownRequestedForReaderEvent: TelemetryEvent
    {
        public GracefulShutdownRequestedForReaderEvent(string readerName, string eventType)
        {
            ReaderName = readerName;
            EventType = eventType;
        }

        public string ReaderName { get; }

        public string EventType { get; }
    }
}