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
        public string Selector { get; set; }

        public UpsertWebhookRequest(string eventName, string subscriberName, string selector, EndpointDto dto)
        {
            EventName = eventName;
            SubscriberName = subscriberName;
            Selector = selector;
            Endpoint = dto;
        }
    }
}