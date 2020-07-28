using System;
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
        public Task<OperationResult<IEnumerable<SubscriberEntity>>> GetSubscribersListAsync(string eventName);

        /// <summary>
        /// Get particular subscriber for a specified event
        /// </summary>
        /// <param name="subscriberId">The id of the subscriber</param>
        /// <returns></returns>
        public Task<OperationResult<SubscriberEntity>> GetSubscriberAsync(SubscriberId subscriberId);

        /// <summary>
        /// Get all the subscribers
        /// </summary>
        /// <returns></returns>
        public Task<OperationResult<IEnumerable<SubscriberEntity>>> GetAllSubscribersAsync();

        /// <summary>
        /// Saves Subscriber
        /// </summary>
        /// <param name="subscriberEntity">Subscriber entity to Save</param>
        /// <returns></returns>
        public Task<OperationResult<Guid>> SaveSubscriberAsync(SubscriberEntity subscriberEntity);
    }
}
