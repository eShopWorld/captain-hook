using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Results;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public class LoadingSubscribersResult
    {
        public IEnumerable<ErrorBase> Errors { get; }
        public IEnumerable<SubscriberConfiguration> Subscribers { get; }
        public bool HasErrors => Errors.Any();

        public LoadingSubscribersResult(params ErrorBase[] errors)
            : this(Enumerable.Empty<SubscriberConfiguration>(), errors)
        {
        }

        public LoadingSubscribersResult(params SubscriberConfiguration[] subscribers)
            : this(subscribers, Enumerable.Empty<ErrorBase>())
        {
        }

        public LoadingSubscribersResult(IEnumerable<SubscriberConfiguration> subscribers, IEnumerable<ErrorBase> errors)
        {
            Subscribers = subscribers ?? throw new ArgumentNullException(nameof(subscribers));
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}