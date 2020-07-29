using CaptainHook.Domain.Entities;
using Eshopworld.Data.CosmosDb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
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

        public Task<OperationResult<SubscriberEntity>> GetSubscriberAsync(SubscriberId subscriberId)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult<IEnumerable<SubscriberEntity>>> GetAllSubscribersAsync()
        {
            var query = _endpointQueryBuilder.BuildSelectAllSubscribersEndpoints();
            var endpoints = await _cosmosDbRepository.QueryAsync<EndpointDocument>(query);

            return endpoints
                .GroupBy(x => new { x.EventName, x.SubscriberName })
                .Select(x => Map(x))
                .ToList();
        }

        public Task<OperationResult<SubscriberEntity>> SaveSubscriberAsync(SubscriberEntity subscriberEntity)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult<IEnumerable<SubscriberEntity>>> GetSubscribersListAsync(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            var subscribers = await GetSubscribersListInternalAsync(eventName);

            var materialized = subscribers.ToList();
            if (!materialized.Any())
            {
                return new EntityNotFoundError(nameof(SubscriberEntity), eventName);
            }

            return materialized;
        }

        #region Private methods

        private async Task<IEnumerable<SubscriberEntity>> GetSubscribersListInternalAsync(string eventName)
        {
            var query = _endpointQueryBuilder.BuildSelectSubscribersListEndpoints(eventName);
            var endpoints = await _cosmosDbRepository.QueryAsync<EndpointDocument>(query);

            return endpoints
                .GroupBy(x => new { x.EventName, x.SubscriberName })
                .Select(x => Map(x))
                .ToList();
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
            var endpointDocuments = endpoints.ToArray();

            var endpointDocument = endpointDocuments.First();
            var webhookSelectionRule = endpointDocuments
                .FirstOrDefault(x => x.WebhookType == WebhookType.Webhook)?
                .WebhookSelectionRule;

            var eventEntity = new EventEntity(endpointDocument.EventName);

            var subscriber = new SubscriberEntity(
                endpointDocument.SubscriberName,
                webhookSelectionRule,
                eventEntity);

            foreach (var endpoint in endpointDocuments)
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
