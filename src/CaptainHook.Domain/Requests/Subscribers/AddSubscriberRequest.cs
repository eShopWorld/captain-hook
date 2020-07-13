﻿using System;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Domain.Requests.Subscribers
{
    public class AddSubscriberRequest : IRequest<EitherErrorOr<Guid>>
    {
        public string Name { get; set; }
        public string EventName { get; set; }
    }
}