using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class ReaderDeleteError : ErrorBase
    {
        public ReaderDeleteError(string subscriberName, string eventName)
           : base($"Can't delete Reader Service for Event {eventName} and Subscriber {subscriberName}")
        {
        }
    }
}