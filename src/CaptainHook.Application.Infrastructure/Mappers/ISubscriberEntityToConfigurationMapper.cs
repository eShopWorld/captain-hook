using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public interface ISubscriberEntityToConfigurationMapper
    {
        /// <summary>
        /// Map a subscriber entity to a subscriber configuration
        /// </summary>
        /// <param name="entity">A subscriber entity</param>
        /// <returns>A subscriber configuration result or error</returns>
        Task<OperationResult<IEnumerable<SubscriberConfiguration>>> MapSubscriberAsync(SubscriberEntity entity);
    }
}