﻿using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService
{
    public class SubscriberConfigurationComparer
    {
        public ComparisionResult Compare(IDictionary<string, SubscriberConfiguration> oldConfig, IDictionary<string, SubscriberConfiguration> newConfig)
        {
            var added = new Dictionary<string, SubscriberConfiguration>(newConfig.Where(x => !oldConfig.Keys.Contains(x.Key)));
            var removed = new Dictionary<string, SubscriberConfiguration>(oldConfig.Where(x => !newConfig.Keys.Contains(x.Key)));

            var changed = new Dictionary<string, SubscriberConfiguration>();
            var commonKeys = oldConfig.Keys.Intersect(newConfig.Keys);
            foreach (var key in commonKeys)
            {
                var previous = JsonConvert.SerializeObject(oldConfig[key]);
                var current = JsonConvert.SerializeObject(newConfig[key]);

                if (previous != current)
                {
                    changed.Add(key, newConfig[key]);
                }
            }

            return new ComparisionResult(added, removed, changed);
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
