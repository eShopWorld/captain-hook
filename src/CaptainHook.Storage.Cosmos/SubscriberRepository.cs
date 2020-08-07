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
using Eshopworld.Core;

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
        private readonly IBigBrother _bigBrother;


        public string CollectionName { get; } = "subscribers";

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriberRepository" /> class.
        /// </summary>
        /// <param name="cosmosDbRepository">The Cosmos DB repository</param>
        /// <param name="setup">The setup</param>
        /// <param name="queryBuilder">The query builder</param>
        /// <param name="bigBrother">A BigBrother instance</param>
        /// <exception cref="System.ArgumentNullException">If cosmosDbRepository is null</exception>
        /// <exception cref="System.ArgumentNullException">If endpointQueryBuilder is null</exception>
        public SubscriberRepository(
            ICosmosDbRepository cosmosDbRepository,
            ISubscriberQueryBuilder queryBuilder,
            IBigBrother bigBrother)
        {
            _cosmosDbRepository = cosmosDbRepository ?? throw new ArgumentNullException(nameof(cosmosDbRepository));
            _endpointQueryBuilder = queryBuilder ?? throw new ArgumentNullException(nameof(queryBuilder));
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));

            _cosmosDbRepository.UseCollection(CollectionName);
        }

        public Task<OperationResult<SubscriberEntity>> GetSubscriberAsync(SubscriberId subscriberId)
        {
            if (subscriberId == null)
            {
                throw new ArgumentNullException(nameof(subscriberId));
            }

            return GetSubscriberInternalAsync(subscriberId);
        }

        public async Task<OperationResult<IEnumerable<SubscriberEntity>>> GetAllSubscribersAsync()
        {
            try
            {
                var query = _endpointQueryBuilder.BuildSelectAllSubscribers();
                var subscribers = await _cosmosDbRepository.QueryAsync<SubscriberDocument>(query);

                return subscribers
                    .Select(Map)
                    .ToList();
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
                return new CannotQueryEntityError(nameof(SubscriberEntity), exception);
            }
        }

        public Task<OperationResult<SubscriberEntity>> AddSubscriberAsync(SubscriberEntity subscriberEntity)
        {
            if (subscriberEntity == null)
            {
                throw new ArgumentNullException(nameof(subscriberEntity));
            }

            return AddSubscriberInternalAsync(subscriberEntity);
        }

        public Task<OperationResult<IEnumerable<SubscriberEntity>>> GetSubscribersListAsync(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            return GetSubscribersListInternalAsync(eventName);
        }

        public Task<OperationResult<SubscriberEntity>> UpdateSubscriberAsync(SubscriberEntity subscriberEntity)
        {
            if (subscriberEntity == null)
            {
                throw new ArgumentNullException(nameof(subscriberEntity));
            }

            return UpdateSubscriberInternalAsync(subscriberEntity);
        }

        #region Private methods
        private async Task<OperationResult<SubscriberEntity>> UpdateSubscriberInternalAsync(SubscriberEntity subscriberEntity)
        {
            try
            {
                var subscriberDocument = Map(subscriberEntity);

                var result = await _cosmosDbRepository.UpsertAsync(subscriberDocument);
                return Map(result.Document);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
                return new CannotUpdateEntityError(nameof(SubscriberEntity));
            }
        }

        private async Task<OperationResult<SubscriberEntity>> AddSubscriberInternalAsync(SubscriberEntity subscriberEntity)
        {
            try
            {
                var subscriberDocument = Map(subscriberEntity);

                var result = await _cosmosDbRepository.CreateAsync(subscriberDocument);
                return Map(result.Document);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
                return new CannotSaveEntityError(nameof(SubscriberEntity));
            }
        }

        private async Task<OperationResult<SubscriberEntity>> GetSubscriberInternalAsync(SubscriberId subscriberId)
        {
            try
            {
                var query = _endpointQueryBuilder.BuildSelectSubscriber(subscriberId, subscriberId.EventName);
                var subscribers = await _cosmosDbRepository.QueryAsync<SubscriberDocument>(query);

                if (!subscribers.Any())
                {
                    return new EntityNotFoundError(nameof(SubscriberEntity), subscriberId);
                }

                return subscribers
                    .Select(x => Map(x))
                    .First();
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
                return new CannotQueryEntityError(nameof(SubscriberEntity), exception);
            }
        }

        private async Task<OperationResult<IEnumerable<SubscriberEntity>>> GetSubscribersListInternalAsync(string eventName)
        {
            try
            {
                var query = _endpointQueryBuilder.BuildSelectForEventSubscribers(eventName);
                var subscribers = await _cosmosDbRepository.QueryAsync<SubscriberDocument>(query);

                if (!subscribers.Any())
                {
                    return new EntityNotFoundError(nameof(SubscriberEntity), eventName);
                }

                return subscribers
                    .Select(x => Map(x))
                    .ToList();
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
                return new CannotQueryEntityError(nameof(SubscriberEntity), exception);
            }
        }

        private SubscriberDocument Map(SubscriberEntity subscriberEntity)
        {
            return new SubscriberDocument
            {
                Id = subscriberEntity.Id,
                EventName = subscriberEntity.ParentEvent.Name,
                SubscriberName = subscriberEntity.Name,
                Webhooks = Map(subscriberEntity.Webhooks)
            };
        }

        private WebhookSubdocument Map(WebhooksEntity webhooksEntity)
        {
            var endpoints = 
                webhooksEntity.Endpoints?.Select(webhookEndpoint => Map(webhookEndpoint))
                ?? Enumerable.Empty<EndpointSubdocument>();

            return new WebhookSubdocument
            {
                SelectionRule = webhooksEntity.SelectionRule,
                Endpoints = endpoints.ToArray()
            };
        }

        private EndpointSubdocument Map(EndpointEntity endpointEntity)
        {
            return new EndpointSubdocument
            {
                Selector = endpointEntity.Selector,
                HttpVerb = endpointEntity.HttpVerb,
                Uri = endpointEntity.Uri,
                Authentication = Map(endpointEntity.Authentication),
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

            subscriberEntity.AddWebhooks(Map(subscriberDocument.Webhooks, subscriberEntity));

            return subscriberEntity;
        }

        private WebhooksEntity Map(WebhookSubdocument webhookSubdocument, SubscriberEntity subscriberEntity)
        {
            var webhookEntity = new WebhooksEntity(webhookSubdocument.SelectionRule);
            foreach(var endpointSubdocument in webhookSubdocument.Endpoints)
            {
                webhookEntity.AddEndpoint(Map(endpointSubdocument, subscriberEntity));
            }

            return webhookEntity;
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
