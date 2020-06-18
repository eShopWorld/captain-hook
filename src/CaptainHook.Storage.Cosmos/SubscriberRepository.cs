using CaptainHook.Domain.Models;
using CaptainHook.Repository.Models;
using Eshopworld.Data.CosmosDb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Repository.QueryBuilders;
using System.Linq;
using CaptainHook.Domain.Interfaces;

namespace CaptainHook.Repository
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

        public Task<IEnumerable<SubscriberModel>> GetSubscribersListAsync(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            return GetSubscribersListInternalAsync(eventName);
        }

        #region Private methods

        private async Task<IEnumerable<SubscriberModel>> GetSubscribersListInternalAsync(string eventName)
        {
            var query = _endpointQueryBuilder.BuildSelectSubscribersListEndpoints(eventName);
            var endpoints = await _cosmosDbRepository.QueryAsync<EndpointDocument>(query);

            return endpoints
                .GroupBy(x => x.SubscriberName)
                .Select(x => Map(x));
        }

        private EndpointModel Map(EndpointDocument endpoint)
        {
            var authentication = Map(endpoint.Authentication);
            return new EndpointModel(endpoint.Uri, authentication, endpoint.HttpVerb, endpoint.EndpointSelector);
        }

        private AuthenticationModel Map(AuthenticationData authentication)
        {
            var secretStore = new SecretStoreModel(authentication.KeyVaultName, authentication.SecretName);
            return new AuthenticationModel(authentication.ClientId, secretStore, authentication.Uri, authentication.Type, authentication.Scopes);
        }

        private SubscriberModel Map(IEnumerable<EndpointDocument> endpoints)
        {
            var subscriberName = endpoints.First().SubscriberName;
            var webhookSelectionRule = endpoints
                .FirstOrDefault(x => x.WebhookType == WebhookType.Webhook)?
                .WebhookSelectionRule;

            var subscriber = new SubscriberModel(subscriberName, webhookSelectionRule);

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
