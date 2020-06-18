using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.Domain.Models
{
    /// <summary>
    /// Event model
    /// </summary>
    public class EventModel        
    {
        /// <summary>
        /// Event name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// List all the event subscribers
        /// </summary>
        public IEnumerable<SubscriberModel> Subscribers => _subscribers;

        private readonly List<SubscriberModel> _subscribers;

        public EventModel() : this(null, null) { }

        public EventModel(string name): this(name, null) { }

        public EventModel(string name, IEnumerable<SubscriberModel> subscribers)
        {
            _subscribers = subscribers?.ToList() ?? new List<SubscriberModel>();
            Name = name;
        }
    }
}
