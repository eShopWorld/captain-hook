using System;
using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Requests.Subscribers
{
    public class UpsertWebhookRequest : IRequest<OperationResult<Guid>>
    {
        public string EventName { get; set; }
        public string SubscriberName { get; set; }
        public EndpointDto Endpoint { get; set; }

        public UpsertWebhookRequest(string eventName, string subscriberName, EndpointDto dto)
        {
            EventName = eventName;
            SubscriberName = subscriberName;
            Endpoint = dto;
        }
    }
}