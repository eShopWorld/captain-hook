using CaptainHook.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaptainHook.Domain.Interfaces
{
    public interface ISubscriberRepository
    {
        /// <summary>
        /// Get all the subscribers for a specified event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <returns></returns>
        public Task<IEnumerable<SubscriberModel>> GetSubscribersListAsync(string eventName);
    }
}
