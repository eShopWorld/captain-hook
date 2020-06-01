using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService
{
    public class SubscriberConfigurationComparer
    {
        public Result Compare(IDictionary<string, SubscriberConfiguration> oldConfig, IDictionary<string, SubscriberConfiguration> newConfig)
        {
            var added = new Dictionary<string, SubscriberConfiguration>(newConfig.Where(x => !oldConfig.Keys.Contains(x.Key)));
            var removed = new Dictionary<string, SubscriberConfiguration>(oldConfig.Where(x => !newConfig.Keys.Contains(x.Key)));

            var changed = new Dictionary<string, SubscriberConfiguration>();
            var keysToCheck = oldConfig.Keys.Intersect(newConfig.Keys);
            foreach (var key in keysToCheck)
            {
                var previous = JsonConvert.SerializeObject(oldConfig[key]);
                var current = JsonConvert.SerializeObject(newConfig[key]);

                if (previous != current)
                {
                    changed.Add(key, newConfig[key]);
                }
            }

            return new Result(added, removed, changed);
        }

        public class Result
        {
            public IReadOnlyDictionary<string, SubscriberConfiguration> Added { get; }
            public IReadOnlyDictionary<string, SubscriberConfiguration> Removed { get; }
            public IReadOnlyDictionary<string, SubscriberConfiguration> Changed { get; }
            public bool HasChanged => this.Added.Any() || this.Removed.Any() || this.Changed.Any();

            public Result(IReadOnlyDictionary<string, SubscriberConfiguration> added, 
                IReadOnlyDictionary<string, SubscriberConfiguration> removed, IReadOnlyDictionary<string, SubscriberConfiguration> changed)
            {
                Added = added;
                Removed = removed;
                Changed = changed;
            }
        }
    }
}