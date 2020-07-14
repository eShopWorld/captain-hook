using System.Collections.Generic;
using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Application.Requests.Subscribers
{
    public class GetSubscribersForEventQuery : IRequest<EitherErrorOr<List<SubscriberDto>>>
    {
        public string Name { get; set; }
    }
}