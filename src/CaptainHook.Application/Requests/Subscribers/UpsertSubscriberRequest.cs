using System;
using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Requests.Subscribers
{
    public class UpsertSubscriberRequest : IRequest<OperationResult<Guid>>
    {
        public string EventName { get; set; }
        public string SubscriberName { get; set; }
        public SubscriberDto SubscriberDto { get; set; }

        public UpsertSubscriberRequest(string eventName, string subscriberName, SubscriberDto subscriberDto)
        {
            EventName = eventName;
            SubscriberName = subscriberName;
            SubscriberDto = subscriberDto;
        }
    }
}