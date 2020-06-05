using Eshopworld.Core;

namespace CaptainHook.DirectorService.Events
{
    class ServiceDeletedEvent : TelemetryEvent
    {
        public string ReaderName { get; set; }

        public ServiceDeletedEvent(string readerName)
        {
            ReaderName = readerName;
        }
    }
}