using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class ConfigurationMerger : IConfigurationMerger
    {
        private readonly ISubscriberEntityToConfigurationMapper _subscriberEntityToConfigurationMapper;

        public ConfigurationMerger(ISubscriberEntityToConfigurationMapper subscriberEntityToConfigurationMapper)
        {
            _subscriberEntityToConfigurationMapper = subscriberEntityToConfigurationMapper;
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
                    kvSubscriber.EventType.Equals(cosmosSubscriber.ParentEvent.Name, StringComparison.InvariantCultureIgnoreCase)
                    && kvSubscriber.SubscriberName.Equals(cosmosSubscriber.Name, StringComparison.InvariantCultureIgnoreCase)));

            async Task<OperationResult<IEnumerable<SubscriberConfiguration>>> MapCosmosEntries()
            {
                var tasks = subscribersFromCosmos.Select(_subscriberEntityToConfigurationMapper.MapSubscriberAsync).ToArray();
                await Task.WhenAll(tasks);

                var errors = tasks.Select(x => x.Result).Where(x => x.IsError);

                if(errors.Any())
                {
                    var failures = errors.SelectMany(x => x.Error.Failures).ToArray();
                    return new MappingError("Cannot map Cosmos DB entries", failures);
                }

                return tasks.SelectMany(t => t.Result.Data).ToList();
            }

            var fromCosmosResult = await MapCosmosEntries();

            if(fromCosmosResult.IsError)
            {
                return fromCosmosResult.Error;
            }

            var union = onlyInKv.Union(fromCosmosResult.Data).ToList();
            return new ReadOnlyCollection<SubscriberConfiguration>(union);
        }
    }
}
