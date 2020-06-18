using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Event model
    /// </summary>
    public class EventEntity        
    {
        /// <summary>
        /// Event name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// List all the event subscribers
        /// </summary>
        public IEnumerable<SubscriberEntity> Subscribers => _subscribers;

        private readonly List<SubscriberEntity> _subscribers;

        public EventEntity() : this(null, null) { }

        public EventEntity(string name): this(name, null) { }

        public EventEntity(string name, IEnumerable<SubscriberEntity> subscribers)
        {
            _subscribers = subscribers?.ToList() ?? new List<SubscriberEntity>();
            Name = name;
        }
    }
}
