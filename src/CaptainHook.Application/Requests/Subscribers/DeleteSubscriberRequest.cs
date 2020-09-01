using System;
using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Requests.Subscribers
{
    public class DeleteSubscriberRequest : IRequest<OperationResult<bool>>
    {
        public string EventName { get; }
        public string SubscriberName { get; }

        public DeleteSubscriberRequest(string eventName, string subscriberName)
        {
            EventName = eventName;
            SubscriberName = subscriberName;
        }
    }
}