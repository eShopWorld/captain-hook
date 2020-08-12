using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public interface ISubscriberEntityToConfigurationMapper
    {
        /// <summary>
        /// Map a subscriber entity to a subscriber configuration
        /// </summary>
        /// <param name="entity">A subscriber entity</param>
        /// <returns>A subscriber configuration</returns>
        Task<IEnumerable<SubscriberConfiguration>> MapSubscriberAsync(SubscriberEntity entity);
    }
}