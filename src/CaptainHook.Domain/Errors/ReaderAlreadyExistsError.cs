using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class ReaderAlreadyExistsError : ErrorBase
    {
        public ReaderAlreadyExistsError(string subscriberName, string eventName)
           : base($"Can't create Reader Service for Event {eventName} and Subscriber {subscriberName} because it already exist.")
        {
        }
    }
}