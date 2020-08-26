using CaptainHook.Application.Results;
using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Requests.Subscribers
{
    public class UpsertSubscriberRequest : IRequest<OperationResult<UpsertResult<SubscriberDto>>>
    {
        public string EventName { get; }
        public string SubscriberName { get; }
        public SubscriberDto Subscriber { get; }

        public UpsertSubscriberRequest(string eventName, string subscriberName, SubscriberDto dto)
        {
            EventName = eventName;
            SubscriberName = subscriberName;
            Subscriber = dto;
        }
    }
}
