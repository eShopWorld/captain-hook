using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class ReaderCreateError : ErrorBase
    {
        public ReaderCreateError(string subscriberName, string eventName)
           : base($"Can't create Reader Service for Event {eventName} and Subscriber {subscriberName}")
        {
        }
    }
}