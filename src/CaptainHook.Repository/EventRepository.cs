using CaptainHook.Domain.Models;
using CaptainHook.Repository.Models;
using Eshopworld.Data.CosmosDb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Repository.QueryBuilders;
using System.Linq;

namespace CaptainHook.Repository
{
    /// <summary>
    /// Event repository
    /// </summary>
    /// <seealso cref="IEventRepository" />
    public class EventRepository : IEventRepository
    {
        private readonly ICosmosDbRepository _cosmosDbRepository;
        private readonly IEventQueryBuilder _endpointQueryBuilder;

        public string CollectionName { get; } = "endpoints";
        //public string GenerateId(Endpoint entity) => Guid.NewGuid().ToString();
        //public string ResolvePartitionKey(Endpoint entity) => $"{entity.EventName}-{entity.SubscriberName}";

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRepository" /> class.
        /// </summary>
        /// <param name="cosmosDbRepository">The Cosmos DB repository</param>
        /// <param name="setup">The setup</param>
        /// <param name="queryBuilder">The query builder</param>
        /// <exception cref="System.ArgumentNullException">If cosmosDbRepository is null</exception>
        /// <exception cref="System.ArgumentNullException">If endpointQueryBuilder is null</exception>
        public EventRepository(ICosmosDbRepository cosmosDbRepository, IEventQueryBuilder queryBuilder)
        {
            _cosmosDbRepository = cosmosDbRepository ?? throw new ArgumentNullException(nameof(cosmosDbRepository));
            _endpointQueryBuilder = queryBuilder ?? throw new ArgumentNullException(nameof(queryBuilder));

            _cosmosDbRepository.UseCollection(CollectionName);
        }

        public Task<SubscriberModel> GetSubscriberAsync(string eventName, string subscriberName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            if (string.IsNullOrWhiteSpace(subscriberName))
            {
                throw new ArgumentNullException(nameof(subscriberName));
            }

            return GetSubscriberInternalAsync(eventName, subscriberName);
        }

        public Task<IEnumerable<SubscriberModel>> GetEventSubscribersAsync(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            return GetSubscribersInternalAsync(eventName);
        }

        #region Private methods

        private async Task<IEnumerable<SubscriberModel>> GetSubscribersInternalAsync(string eventName)
        {
            var query = _endpointQueryBuilder.BuildSelectSubscribersEndpoints(eventName);
            var endpoints = await _cosmosDbRepository.QueryAsync<Endpoint>(query);

            return endpoints
                .GroupBy(x => x.SubscriberName)
                .Select(x => Map(x));
        }

        private async Task<SubscriberModel> GetSubscriberInternalAsync(string eventName, string subscriberName)
        {
            var query = _endpointQueryBuilder.BuildSelectSubscriberEndpoints(eventName, subscriberName);
            var endpoints = await _cosmosDbRepository.QueryAsync<Endpoint>(query);
            return Map(endpoints);
        }

        private EndpointModel Map(Endpoint endpoint)
        {
            var authentication = Map(endpoint.Authentication);
            return new EndpointModel(endpoint.Uri, authentication, endpoint.HttpVerb, endpoint.EndpointSelector);
        }

        private AuthenticationModel Map(Authentication authentication)
        {
            var secretStore = new SecretStoreModel(authentication.KeyVaultName, authentication.SecretName);
            return new AuthenticationModel(authentication.ClientId, secretStore, authentication.Uri, authentication.Type, authentication.Scopes);
        }

        private SubscriberModel Map(IEnumerable<Endpoint> endpoints)
        {
            var subscriberName = endpoints.First().SubscriberName;
            var webhookSelector = endpoints
                .FirstOrDefault(x => x.WebhookType == WebhookType.Webhook)?
                .WebhookSelector;
            var callbackSelector = endpoints
                .FirstOrDefault(x => x.WebhookType == WebhookType.Callback)?
                .WebhookSelector;
            var dqlSelector = endpoints
                .FirstOrDefault(x => x.WebhookType == WebhookType.Dlq)?
                .WebhookSelector;

            var subscriber = new SubscriberModel(subscriberName, webhookSelector, callbackSelector, dqlSelector);

            foreach (var endpoint in endpoints)
            {
                var domainEndpoint = Map(endpoint);
                switch (endpoint.WebhookType)
                {
                    case WebhookType.Webhook:
                        subscriber.AddWebhookEndpoint(domainEndpoint);
                        break;
                    case WebhookType.Callback:
                        subscriber.AddCallbackEndpoint(domainEndpoint);
                        break;
                    case WebhookType.Dlq:
                        subscriber.AddDlqEndpoint(domainEndpoint);
                        break;
                }
            }

            return subscriber;
        }

        #endregion

        //public async Task<Endpoint> AddAsync(Endpoint entity)
        //{
        //    try
        //    {
        //        entity.Id = GenerateId(entity);
        //        entity.Pk = ResolvePartitionKey(entity);
        //        var document = await _cosmosDbRepository.CreateAsync(entity);
        //        return document;
        //    }
        //    catch (CosmosException e)
        //    {
        //        if (e.StatusCode == HttpStatusCode.Conflict)
        //        {
        //            throw new EndpointAlreadyExistsException();
        //        }

        //        throw;
        //    }
        //}

        //public async Task DeleteAsync(Endpoint entity)
        //{
        //    await _cosmosDbRepository.DeleteAsync<Endpoint>(entity.Id, ResolvePartitionKey(entity));
        //}

        //public async Task<IEnumerable<Endpoint>> GetEndpointsBySubscriber(string eventName, string subscriberName)
        //{
        //    var partitionKey = $"{eventName}-{subscriberName}";
        //    var query = new QueryDefinition("select * from c");
        //    return await _cosmosDbRepository.QueryAsync<Endpoint>(new CosmosQuery(query, partitionKey));
        //}
    }
}
