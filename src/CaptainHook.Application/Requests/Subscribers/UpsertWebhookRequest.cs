using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Requests.Subscribers
{
    public class UpsertWebhookRequest : IRequest<OperationResult<EndpointDto>>
    {
        public string EventName { get; }
        public string SubscriberName { get; }
        public EndpointDto Endpoint { get; }

        public UpsertWebhookRequest(string eventName, string subscriberName, EndpointDto dto)
        {
            EventName = eventName;
            SubscriberName = subscriberName;
            Endpoint = dto;
        }
    }
}