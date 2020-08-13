using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Requests.Subscribers
{
    public class DeleteWebhookRequest : IRequest<OperationResult<EndpointDto>>
    {
        public string EventName { get; }
        public string SubscriberName { get; }

        public DeleteWebhookRequest(string eventName, string subscriberName)
        {
            EventName = eventName;
            SubscriberName = subscriberName;
        }
    }
}