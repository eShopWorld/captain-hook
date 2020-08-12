using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class ReaderDoesNotExistError : ErrorBase
    {
        public ReaderDoesNotExistError(SubscriberEntity subscriber)
            : base($"Can't update Reader Service for Event {subscriber?.ParentEvent?.Name} and Subscriber {subscriber?.Name} because it doesn't exist.")
        {
        }
    }
}