using CaptainHook.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaptainHook.Repository
{
    public interface IEventRepository
    {
        /// <summary>
        /// Get all the subscribers for a specified event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <returns></returns>
        public Task<IEnumerable<SubscriberModel>> GetEventSubscribersAsync(string eventName);

        /// <summary>
        /// Get a single subscriber
        /// </summary>
        /// <param name="eventName">The subscriber event name</param>
        /// <param name="subscriberName">The subscriber name</param>
        /// <returns></returns>
        public Task<SubscriberModel> GetSubscriberAsync(string eventName, string subscriberName);
    }
}
