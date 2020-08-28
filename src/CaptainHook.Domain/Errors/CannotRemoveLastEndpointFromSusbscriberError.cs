using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class CannotRemoveLastEndpointFromSubscriberError: ErrorBase
    {
        public CannotRemoveLastEndpointFromSubscriberError()
            : base("Cannot remove last endpoint from collection")
        {
        }

        public CannotRemoveLastEndpointFromSubscriberError(SubscriberEntity entity)
            : base($"Cannot remove last endpoint from Subscriber: {entity.Id}")
        {
        }
    }
}