using Eshopworld.Core;

namespace CaptainHook.EventReaderService.Events
{
    class ServiceDeletedEvent : TelemetryEvent
    {
        public string ReaderName { get; }

        public ServiceDeletedEvent(string readerName)
        {
            ReaderName = readerName;
        }
    }
}