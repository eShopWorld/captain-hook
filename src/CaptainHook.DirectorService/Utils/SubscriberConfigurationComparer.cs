using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CaptainHook.Common.Configuration;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.Utils
{
    public class SubscriberConfigurationComparer
    {
        /// <summary>
        /// Compares previous and current subscriber configurations.
        /// </summary>
        /// <param name="oldConfig">Old configuration - the one which is currently deployed.</param>
        /// <param name="newConfig">New configuration - the one which is up to date, but hasn't been deployed yet.</param>
        /// <returns>A comparision result which contains three collections of subscriber configurations:
        /// 1. Added - new subscribers which don't exist in old configuration list but exist in new.
        /// 2. Removed - subscribers whose exist in old configurations list but are absent in new configuration.
        /// 3. Changes - subscribers whose exist in both configurations, but some of configuration parameters has been changed.
        /// </returns>
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

            public Result(IDictionary<string, SubscriberConfiguration> added, IDictionary<string, SubscriberConfiguration> removed, IDictionary<string, SubscriberConfiguration> changed)
            {
                Added = new ReadOnlyDictionary<string, SubscriberConfiguration>(added);
                Removed = new ReadOnlyDictionary<string, SubscriberConfiguration>(removed);
                Changed = new ReadOnlyDictionary<string, SubscriberConfiguration>(changed);
            }
        }
    }
}