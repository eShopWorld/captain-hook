using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public interface ISubscriberEntityToConfigurationMapper
    {
        /// <summary>
        /// Map a subscriber entity to a subscriber configuration which contain webhook and optional callback 
        /// </summary>
        /// <param name="entity">A subscriber entity</param>
        /// <returns>A subscriber configuration result or error</returns>
        Task<OperationResult<SubscriberConfiguration>> MapToWebhookAsync(SubscriberEntity entity);

        /// <summary>
        /// Map a subscriber entity to a subscriber configuration which contain DLQ
        /// </summary>
        /// <param name="entity">A subscriber entity</param>
        /// <returns>A subscriber configuration result or error</returns>
        Task<OperationResult<SubscriberConfiguration>> MapToDlqAsync(SubscriberEntity entity);
    }
}