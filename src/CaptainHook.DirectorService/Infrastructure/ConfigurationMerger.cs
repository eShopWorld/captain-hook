using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Events;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using Eshopworld.Core;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class ConfigurationMerger : IConfigurationMerger
    {
        private readonly ISubscriberEntityToConfigurationMapper _subscriberEntityToConfigurationMapper;
        private readonly IBigBrother _bigBrother;

        public ConfigurationMerger(ISubscriberEntityToConfigurationMapper subscriberEntityToConfigurationMapper, IBigBrother bigBrother)
        {
            _subscriberEntityToConfigurationMapper = subscriberEntityToConfigurationMapper;
            _bigBrother = bigBrother;
        }

        /// <summary>
        /// Merges subscribers loaded from Cosmos and from KeyVault. If particular subscriber is defined in both sources, the Cosmos version overrides the KeyVault version.
        /// </summary>
        /// <param name="subscribersFromKeyVault">Subscriber definitions loaded from KeyVault</param>
        /// <param name="subscribersFromCosmos">Subscriber models retrieved from Cosmos</param>
        /// <returns>List of all subscribers converted to KeyVault structure</returns>
        public async Task<OperationResult<ReadOnlyCollection<SubscriberConfiguration>>> MergeAsync(
            IEnumerable<SubscriberConfiguration> subscribersFromKeyVault,
            IEnumerable<SubscriberEntity> subscribersFromCosmos)
        {
            var onlyInKv = subscribersFromKeyVault
                .Where(kvSubscriber => !subscribersFromCosmos.Any(cosmosSubscriber =>
                    kvSubscriber.EventType.Equals(cosmosSubscriber.ParentEvent.Name, StringComparison.InvariantCultureIgnoreCase) && 
                    (kvSubscriber.SubscriberName.Equals(cosmosSubscriber.Name, StringComparison.InvariantCultureIgnoreCase) ||
                     (!string.IsNullOrEmpty(kvSubscriber.SourceSubscriptionName) && kvSubscriber.SourceSubscriptionName.Equals(cosmosSubscriber.Name, StringComparison.InvariantCultureIgnoreCase)))
                ));

            var mergeSubscribersEvent = new MergeSubscribersEvent(onlyInKv.Select(x => SubscriberConfiguration.Key(x.EventType, x.SubscriberName)));
            _bigBrother.Publish(mergeSubscribersEvent);

            async Task<OperationResult<IEnumerable<SubscriberConfiguration>>> MapCosmosEntries()
            {
                var tasks = new List<Task<OperationResult<SubscriberConfiguration>>>();
                foreach (var entity in subscribersFromCosmos)
                {
                    tasks.Add(_subscriberEntityToConfigurationMapper.MapToWebhookAsync(entity));
                    if (entity.HasDlqHooks)
                    {
                        tasks.Add(_subscriberEntityToConfigurationMapper.MapToDlqAsync(entity));
                    }
                }

                await Task.WhenAll(tasks);

                var errors = tasks.Select(x => x.Result).Where(x => x.IsError);

                if (errors.Any())
                {
                    var failures = errors.SelectMany(x => x.Error.Failures).ToArray();
                    return new MappingError("Cannot map Cosmos DB entries", failures);
                }

                return tasks.Select(t => t.Result.Data).ToList();
            }

            var fromCosmosResult = await MapCosmosEntries();

            if (fromCosmosResult.IsError)
            {
                return fromCosmosResult.Error;
            }

            var union = onlyInKv.Union(fromCosmosResult.Data).ToList();
            return new ReadOnlyCollection<SubscriberConfiguration>(union);
        }
    }
}
