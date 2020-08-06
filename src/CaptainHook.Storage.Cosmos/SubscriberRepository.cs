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

        public string CollectionName { get; } = "subscribers";

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

        public async Task<OperationResult<SubscriberEntity>> GetSubscriberAsync(SubscriberId subscriberId)
        {
            if (subscriberId == null)
            {
                throw new ArgumentNullException(nameof(subscriberId));
            }

            var subscriber = await GetSubscriberInternalAsync(subscriberId);

            if (subscriber == null)
            {
                return new EntityNotFoundError(nameof(SubscriberEntity), subscriberId);
            }

            return subscriber;
        }

        public async Task<OperationResult<IEnumerable<SubscriberEntity>>> GetAllSubscribersAsync()
        {
            var query = _endpointQueryBuilder.BuildSelectAllSubscribers();
            var subscribers = await _cosmosDbRepository.QueryAsync<SubscriberDocument>(query);

            return subscribers
                .Select(Map)
                .ToList();
        }

        public async Task<OperationResult<SubscriberEntity>> AddSubscriberAsync(SubscriberEntity subscriberEntity)
        {
            if (subscriberEntity == null)
            {
                throw new ArgumentNullException(nameof(subscriberEntity));
            }

            try
            {
                return await AddSubscriberInternalAsync(subscriberEntity);
            }
            catch
            {
                return new CannotSaveEntityError(nameof(SubscriberEntity));
            }
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

        private async Task<SubscriberEntity> AddSubscriberInternalAsync(SubscriberEntity subscriberEntity)
        {
            var subscriberDocument = Map(subscriberEntity);

            var result = await _cosmosDbRepository.CreateAsync(subscriberDocument);
            return Map(result.Document);
        }

        private async Task<SubscriberEntity> GetSubscriberInternalAsync(SubscriberId subscriberId)
        {
            var query = _endpointQueryBuilder.BuildSelectSubscriber(subscriberId);
            var subscribers = await _cosmosDbRepository.QueryAsync<SubscriberDocument>(query);

            return subscribers
                .Select(x => Map(x))
                .FirstOrDefault();
        }

        private async Task<IEnumerable<SubscriberEntity>> GetSubscribersListInternalAsync(string eventName)
        {
            var query = _endpointQueryBuilder.BuildSelectSubscribersList(eventName);
            var subscribers = await _cosmosDbRepository.QueryAsync<SubscriberDocument>(query);

            return subscribers
                .Select(x => Map(x))
                .ToList();
        }

        private SubscriberDocument Map(SubscriberEntity subscriberEntity)
        {
            var endpoints =
                subscriberEntity.Webhooks?.Endpoints?.Select(webhookEndpoint => Map(webhookEndpoint, EndpointType.Webhook))
                ?? Enumerable.Empty<EndpointSubdocument>();

            return new SubscriberDocument
            {
                EventName = subscriberEntity.ParentEvent.Name,
                SubscriberName = subscriberEntity.Name,
                Endpoints = endpoints.ToArray()
            };
        }

        private EndpointSubdocument Map(EndpointEntity endpointEntity, EndpointType endpointType)
        {
            return new EndpointSubdocument
            {
                Selector = endpointEntity.Selector,
                HttpVerb = endpointEntity.HttpVerb,
                Uri = endpointEntity.Uri,
                Authentication = Map(endpointEntity.Authentication),
                Type = endpointType,
                UriTransform = Map(endpointEntity.UriTransform)
            };
        }

        private UriTransformDocument Map(UriTransformEntity uriTransform)
        {
            return uriTransform?.Replace != null ? new UriTransformDocument(uriTransform.Replace) : null;
        }

        private SubscriberEntity Map(SubscriberDocument subscriberDocument)
        {
            var eventEntity = new EventEntity(subscriberDocument.EventName);

            var subscriberEntity = new SubscriberEntity(
                subscriberDocument.SubscriberName,
                eventEntity);

            foreach (var endpointDocument in subscriberDocument.Endpoints)
            {
                subscriberEntity.AddWebhookEndpoint(Map(endpointDocument, subscriberEntity));
            }

            return subscriberEntity;
        }

        private EndpointEntity Map(EndpointSubdocument endpoint, SubscriberEntity subscriberEntity)
        {
            var authentication = Map(endpoint.Authentication);
            var uriTransform = Map(endpoint.UriTransform);
            return new EndpointEntity(endpoint.Uri, authentication, endpoint.HttpVerb, endpoint.Selector, subscriberEntity, uriTransform);
        }

        private UriTransformEntity Map(UriTransformDocument uriTransform)
        {
            if (uriTransform?.Replace == null)
            {
                return null;
            }

            return new UriTransformEntity(uriTransform.Replace);
        }

        private AuthenticationEntity Map(AuthenticationData authentication)
        {
            var secretStore = new SecretStoreEntity(authentication.KeyVaultName, authentication.SecretName);
            return new AuthenticationEntity(authentication.ClientId, secretStore, authentication.Uri, authentication.Type, authentication.Scopes);
        }

        private AuthenticationData Map(AuthenticationEntity authenticationEntity)
        {
            return new AuthenticationData
            {
                SecretName = authenticationEntity.SecretStore.SecretName,
                KeyVaultName = authenticationEntity.SecretStore.KeyVaultName,
                Scopes = authenticationEntity.Scopes,
                ClientId = authenticationEntity.ClientId,
                Uri = authenticationEntity.Uri,
                Type = authenticationEntity.Type
            };
        }

        #endregion
    }
}
