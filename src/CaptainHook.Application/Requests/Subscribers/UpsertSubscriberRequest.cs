using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Requests.Subscribers
{
    public class UpsertSubscriberRequest : IRequest<OperationResult<EndpointDto>>
    {
        public string EventName { get; }
        public string SubscriberName { get; }
        public SubscriberType SubscriberType { get; }
        public SubscriberDto Endpoint { get; }

        public UpsertSubscriberRequest(string eventName, string subscriberName, SubscriberType type, SubscriberDto dto)
        {
            EventName = eventName;
            SubscriberName = subscriberName;
            SubscriberType = type;
            Endpoint = dto;
        }
    }
}
