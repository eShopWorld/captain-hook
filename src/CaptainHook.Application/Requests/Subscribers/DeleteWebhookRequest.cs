using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Requests.Subscribers
{
    public class DeleteWebhookRequest : IRequest<OperationResult<SubscriberDto>>
    {
        public string EventName { get; }
        public string SubscriberName { get; }
        public string Selector { get; }

        public DeleteWebhookRequest(string eventName, string subscriberName, string selector)
        {
            EventName = eventName;
            SubscriberName = subscriberName;
            Selector = selector;
        }
    }
}