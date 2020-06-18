using CaptainHook.Domain.Entities;
using Eshopworld.Data.CosmosDb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CaptainHook.Domain.Repositories;
using CaptainHook.Storage.Cosmos.QueryBuilders;
using CaptainHook.Storage.Cosmos.Models;

namespace CaptainHook.Storage.Cosmos
{
    /// <summary>
    /// Event repository
    /// </summary>
    /// <seealso cref="ISubscriberRepository" />
    public class SubscriberRepository : ISubscriberRepository
    {
        private readonly ICosmosDbRepository _cosmosDbRepository;
        private readonly ISubscriberQueryBuilder _endpointQueryBuilder;

        public string CollectionName { get; } = "endpoints";

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriberRepository" /> class.
        /// </summary>
        /// <param name="cosmosDbRepository">The Cosmos DB repository</param>
        /// <param name="setup">The setup</param>
        /// <param name="queryBuilder">The query builder</param>
        /// <exception cref="System.ArgumentNullException">If cosmosDbRepository is null</exception>
        /// <exception cref="System.ArgumentNullException">If endpointQueryBuilder is null</exception>
        public SubscriberRepository(ICosmosDbRepository cosmosDbRepository, ISubscriberQueryBuilder queryBuilder)
        {
            _cosmosDbRepository = cosmosDbRepository ?? throw new ArgumentNullException(nameof(cosmosDbRepository));
            _endpointQueryBuilder = queryBuilder ?? throw new ArgumentNullException(nameof(queryBuilder));

            _cosmosDbRepository.UseCollection(CollectionName);
        }

        public Task<IEnumerable<SubscriberEntity>> GetSubscribersListAsync(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            return GetSubscribersListInternalAsync(eventName);
        }

        #region Private methods

        private async Task<IEnumerable<SubscriberEntity>> GetSubscribersListInternalAsync(string eventName)
        {
            var query = _endpointQueryBuilder.BuildSelectSubscribersListEndpoints(eventName);
            var endpoints = await _cosmosDbRepository.QueryAsync<EndpointDocument>(query);

            return endpoints
                .GroupBy(x => x.SubscriberName)
                .Select(x => Map(x));
        }

        private EndpointEntity Map(EndpointDocument endpoint)
        {
            var authentication = Map(endpoint.Authentication);
            return new EndpointEntity(endpoint.Uri, authentication, endpoint.HttpVerb, endpoint.EndpointSelector);
        }

        private AuthenticationEntity Map(AuthenticationData authentication)
        {
            var secretStore = new SecretStoreEntity(authentication.KeyVaultName, authentication.SecretName);
            return new AuthenticationEntity(authentication.ClientId, secretStore, authentication.Uri, authentication.Type, authentication.Scopes);
        }

        private SubscriberEntity Map(IEnumerable<EndpointDocument> endpoints)
        {
            var subscriberName = endpoints.First().SubscriberName;
            var webhookSelectionRule = endpoints
                .FirstOrDefault(x => x.WebhookType == WebhookType.Webhook)?
                .WebhookSelectionRule;

            var subscriber = new SubscriberEntity(subscriberName, webhookSelectionRule);

            foreach (var endpoint in endpoints)
            {
                var domainEndpoint = Map(endpoint);
                switch (endpoint.WebhookType)
                {
                    case WebhookType.Webhook:
                        subscriber.AddWebhookEndpoint(domainEndpoint);
                        break;
                }
            }

            return subscriber;
        }

        #endregion
    }
}
