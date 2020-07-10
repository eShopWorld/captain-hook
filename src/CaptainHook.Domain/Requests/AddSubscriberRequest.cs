using System;
using System.Collections.Generic;
using CaptainHook.Contract;
using CaptainHook.Domain.Common;
using MediatR;

namespace CaptainHook.Domain.Requests
{
    public class AddSubscriberRequest : IRequest<EitherErrorOr<Guid>>
    {
        public string Name { get; set; }
        public string EventName { get; set; }
    }

    public class GetSubscribersForEventQuery : IRequest<EitherErrorOr<List<SubscriberDto>>>
    {
         public string Name { get; set; }
    }
}