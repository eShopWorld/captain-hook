using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class ReaderDeleteError : ErrorBase
    {
        public ReaderDeleteError(SubscriberEntity subscriber)
            : base($"Can't delete Reader Service for Event {subscriber?.ParentEvent?.Name} and Subscriber {subscriber?.Name}")
        {
        }

        public ReaderDeleteError(string subscriberName, string eventName)
           : base($"Can't delete Reader Service for Event {eventName} and Subscriber {subscriberName}")
        {
        }
    }
}