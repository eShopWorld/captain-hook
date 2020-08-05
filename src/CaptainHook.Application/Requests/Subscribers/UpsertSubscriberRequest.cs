using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Requests.Subscribers
{
    public class UpsertSubscriberRequest : IRequest<OperationResult<EndpointDto>>
    {
        public string EventName { get; }
        public SubscriberDto Subscriber { get; }

        public UpsertSubscriberRequest(string eventName, SubscriberDto dto)
        {
            EventName = eventName;
            Subscriber = dto;
        }
    }
}
