using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaptainHook.Common.Configuration;

namespace CaptainHook.DirectorService
{
    public class SubscriberConfigurationComparer
    {
        public ComparisionResult Compare(IDictionary<string, SubscriberConfiguration> oldConfig, IDictionary<string, SubscriberConfiguration> newConfig)
        {
            var added = new Dictionary<string, SubscriberConfiguration>(newConfig.Except(oldConfig));
            var removed = new Dictionary<string, SubscriberConfiguration>(oldConfig.Except(newConfig));

            var empty = new Dictionary<string, SubscriberConfiguration>();

            return new ComparisionResult(added, removed, empty);
        }
    }

    public class ComparisionResult
    {
        public IDictionary<string, SubscriberConfiguration> Added { get; }
        public IDictionary<string, SubscriberConfiguration> Removed { get; }
        public IDictionary<string, SubscriberConfiguration> Changed { get; }
        public bool HasChanged => this.Added.Any() || this.Removed.Any() || this.Changed.Any();

        public ComparisionResult(IDictionary<string, SubscriberConfiguration> added, IDictionary<string, SubscriberConfiguration> removed, IDictionary<string, SubscriberConfiguration> changed)
        {
            Added = added;
            Removed = removed;
            Changed = changed;
        }
    }
}
