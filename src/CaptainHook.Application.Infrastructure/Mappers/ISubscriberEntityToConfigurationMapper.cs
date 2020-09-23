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
        
        Task<OperationResult<SubscriberConfiguration>> MapToWebhookAsync(SubscriberEntity entity);

        Task<OperationResult<SubscriberConfiguration>> MapToDlqAsync(SubscriberEntity entity);
    }

    //public class MapToKeyVaultResult
    //{
    //    public SubscriberConfiguration Webhook { get; }
    //    public SubscriberConfiguration Dlqhook { get; }

    //    public MapToKeyVaultResult(SubscriberConfiguration webhook)
    //    {
    //        Webhook = webhook;
    //    }

    //    public MapToKeyVaultResult(SubscriberConfiguration webhook, SubscriberConfiguration dlqhook)
    //    {
    //        Webhook = webhook;
    //        Dlqhook = dlqhook;
    //    }
    //}
}