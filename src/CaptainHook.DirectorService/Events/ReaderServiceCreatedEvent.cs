using Eshopworld.Core;

namespace CaptainHook.DirectorService.Events
{
    public class ReaderServiceCreatedEvent : TelemetryEvent
    {
        public string ReaderName { get; set; }

        public ReaderServiceCreatedEvent(string readerName)
        {
            ReaderName = readerName;
        }
    }
}