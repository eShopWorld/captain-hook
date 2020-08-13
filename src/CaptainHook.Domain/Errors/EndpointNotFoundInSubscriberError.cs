using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class EndpointNotFoundInSubscriberError : ErrorBase
    {
        public EndpointNotFoundInSubscriberError(string selector, SubscriberEntity entity) : 
            base($"Endpoint with selector: '{selector}' does not exist in Subscriber: '{entity.Id}'")
        {
        }
    }
}