using System;
using CaptainHook.Domain.Services;
using MediatR;

namespace CaptainHook.Domain.RequestValidators
{
    public class AddSubscriberRequest : IRequest<EitherErrorOr<Guid>>
    {
        public string Name { get; set; }
        public string EventName { get; set; }
    }
}