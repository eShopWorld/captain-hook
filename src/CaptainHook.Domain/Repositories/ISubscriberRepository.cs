using CaptainHook.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;

namespace CaptainHook.Domain.Repositories
{
    public interface ISubscriberRepository
    {
        /// <summary>
        /// Get all the subscribers for a specified event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <returns></returns>
        Task<OperationResult<IEnumerable<SubscriberEntity>>> GetSubscribersListAsync(string eventName);

        /// <summary>
        /// Get particular subscriber for a specified event
        /// </summary>
        /// <param name="subscriberId">The id of the subscriber</param>
        /// <returns></returns>
        Task<OperationResult<SubscriberEntity>> GetSubscriberAsync(SubscriberId subscriberId);

        /// <summary>
        /// Get all the subscribers
        /// </summary>
        /// <returns></returns>
        Task<OperationResult<IEnumerable<SubscriberEntity>>> GetAllSubscribersAsync();

        /// <summary>
        /// Create a new subscriber
        /// </summary>
        /// <param name="subscriberEntity">Subscriber entity to create</param>
        /// <returns></returns>
        Task<OperationResult<SubscriberEntity>> AddSubscriberAsync(SubscriberEntity subscriberEntity);

        /// <summary>
        /// Update a subscriber
        /// </summary>
        /// <param name="subscriberEntity">Subscriber to update</param>
        /// <returns>Subscriber which has been updated</returns>
        Task<OperationResult<SubscriberEntity>> UpdateSubscriberAsync(SubscriberEntity subscriberEntity);

        /// <summary>
        /// Remove a subscriber
        /// </summary>
        /// <param name="subscriberId">Id of subscriber to remove</param>
        /// <returns>Subscriber which has been removed</returns>
        Task<OperationResult<SubscriberId>> RemoveSubscriberAsync(SubscriberId subscriberId);
    }
}
