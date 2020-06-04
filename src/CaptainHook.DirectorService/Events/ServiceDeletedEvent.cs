using Eshopworld.Core;

namespace CaptainHook.DirectorService.Events
{
    class ServiceDeletedEvent : TelemetryEvent
    {
        public string Message { get; set; }
        public string ReaderName { get; set; }

        public ServiceDeletedEvent(string readerName)
        {
            ReaderName = readerName;
            Message = $"Created Service: {readerName}";
        }
    }
}