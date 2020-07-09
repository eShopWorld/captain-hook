using System;
using CaptainHook.Domain.Common;
using MediatR;

namespace CaptainHook.Domain.Requests
{
    public class AddSubscriberRequest : IRequest<EitherErrorOr<Guid>>
    {
        public string Name { get; set; }
        public string EventName { get; set; }
    }
}