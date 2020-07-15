using CaptainHook.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Repositories
{
    public interface ISubscriberRepository
    {
        /// <summary>
        /// Get all the subscribers for a specified event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <returns></returns>
        public Task<OperationResult<IEnumerable<SubscriberEntity>>> GetSubscribersListAsync(string eventName);

        /// <summary>
        /// Get all the subscribers
        /// </summary>
        /// <returns></returns>
        public Task<OperationResult<IEnumerable<SubscriberEntity>>> GetAllSubscribersAsync();
    }
}
