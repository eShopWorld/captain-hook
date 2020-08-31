using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using CaptainHook.Storage.Cosmos.Models;
using CaptainHook.Storage.Cosmos.QueryBuilders;
using Eshopworld.Data.CosmosDb;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaptainHook.Storage.Cosmos
{
    /// <summary>
    /// Event repository
    /// </summary>
    /// <seealso cref="ISubscriberRepository" />
    public class SubscriberRepository : ISubscriberRepository
    {
        private static readonly AuthenticationSubdocumentJsonConverter AuthenticationSubdocumentJsonConverter = new AuthenticationSubdocumentJsonConverter();

        private readonly ICosmosDbRepository _cosmosDbRepository;
        private readonly ISubscriberQueryBuilder _endpointQueryBuilder;

        public string CollectionName { get; } = "subscribers";

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriberRepository" /> class.
        /// </summary>
        /// <param name="cosmosDbRepository">The Cosmos DB repository</param>
        /// <param name="setup">The setup</param>
        /// <param name="queryBuilder">The query builder</param>
        /// <param name="bigBrother">A BigBrother instance</param>
        /// <exception cref="ArgumentNullException">If cosmosDbRepository is null</exception>
        /// <exception cref="ArgumentNullException">If endpointQueryBuilder is null</exception>
        public SubscriberRepository(
            ICosmosDbRepository cosmosDbRepository,
            ISubscriberQueryBuilder queryBuilder)
        {
            _cosmosDbRepository = cosmosDbRepository ?? throw new ArgumentNullException(nameof(cosmosDbRepository));
            _endpointQueryBuilder = queryBuilder ?? throw new ArgumentNullException(nameof(queryBuilder));

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
                var subscribers = await _cosmosDbRepository.QueryAsync<dynamic>(query);

                return subscribers
                    .Select(Deserialize)
                    .Select(Map)
                    .ToList();
            }
            catch (Exception exception)
            {
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

        public Task<OperationResult<SubscriberEntity>> RemoveSubscriberAsync(SubscriberEntity subscriberEntity)
        {
            if (subscriberEntity == null)
            {
                throw new ArgumentNullException(nameof(subscriberEntity));
            }

            if (subscriberEntity.ParentEvent == null)
            {
                throw new ArgumentNullException(nameof(subscriberEntity.ParentEvent));
            }

            if (string.IsNullOrWhiteSpace(subscriberEntity.ParentEvent.Name))
            {
                throw new ArgumentNullException(nameof(subscriberEntity.ParentEvent.Name));
            }

            return RemoveSubscriberInternalAsync(subscriberEntity);
        }

        #region Private methods
        private async Task<OperationResult<SubscriberEntity>> UpdateSubscriberInternalAsync(SubscriberEntity subscriberEntity)
        {
            try
            {
                var subscriberDocument = Map(subscriberEntity);

                var result = await _cosmosDbRepository.ReplaceAsync<dynamic>(subscriberDocument.Id, subscriberDocument, subscriberEntity.Etag);

                return Map(Deserialize(result.Document));
            }
            catch (Exception exception)
            {
                return new CannotUpdateEntityError(nameof(SubscriberEntity), exception);
            }
        }

        private async Task<OperationResult<SubscriberEntity>> AddSubscriberInternalAsync(SubscriberEntity subscriberEntity)
        {
            try
            {
                var subscriberDocument = Map(subscriberEntity);

                var result = await _cosmosDbRepository.CreateAsync<dynamic>(subscriberDocument);
                return Map(Deserialize(result.Document));
            }
            catch (Exception exception)
            {
                return new CannotSaveEntityError(nameof(SubscriberEntity), exception);
            }
        }

        private async Task<OperationResult<SubscriberEntity>> GetSubscriberInternalAsync(SubscriberId subscriberId)
        {
            try
            {
                var query = _endpointQueryBuilder.BuildSelectSubscriber(subscriberId, subscriberId.EventName);
                var subscribers = await _cosmosDbRepository.QueryAsync<dynamic>(query);

                if (!subscribers.Any())
                {
                    return new EntityNotFoundError(nameof(SubscriberEntity), subscriberId);
                }

                return subscribers
                    .Select(Deserialize)
                    .Select(Map)
                    .First();
            }
            catch (Exception exception)
            {
                return new CannotQueryEntityError(nameof(SubscriberEntity), exception);
            }
        }

        private async Task<OperationResult<IEnumerable<SubscriberEntity>>> GetSubscribersListInternalAsync(string eventName)
        {
            try
            {
                var query = _endpointQueryBuilder.BuildSelectForEventSubscribers(eventName);
                var subscribers = await _cosmosDbRepository.QueryAsync<dynamic>(query);

                if (!subscribers.Any())
                {
                    return new EntityNotFoundError(nameof(SubscriberEntity), eventName);
                }

                return subscribers
                    .Select(Deserialize)
                    .Select(Map)
                    .ToList();
            }
            catch (Exception exception)
            {
                return new CannotQueryEntityError(nameof(SubscriberEntity), exception);
            }
        }

        private async Task<OperationResult<SubscriberEntity>> RemoveSubscriberInternalAsync(SubscriberEntity subscriberEntity)
        {
            try
            {
                var pk = SubscriberDocument.GetPartitionKey(subscriberEntity.ParentEvent.Name);
                
                var result = await _cosmosDbRepository.DeleteAsync<SubscriberDocument>(subscriberEntity.Id, pk);
                
                if(result)
                {
                    return subscriberEntity;
                }
                else
                {
                    return new CannotDeleteEntityError(nameof(SubscriberEntity));
                }
            }
            catch (Exception exception)
            {
                return new CannotDeleteEntityError(nameof(SubscriberEntity), exception);
            }
        }

        private static SubscriberDocument Deserialize(dynamic document)
        {
            return JsonConvert.DeserializeObject<SubscriberDocument>(document?.ToString(), AuthenticationSubdocumentJsonConverter);
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
                Authentication = MapToAuthenticationSubdocument(endpointEntity.Authentication)
            };
        }

        private UriTransformSubdocument Map(UriTransformEntity uriTransform)
        {
            return uriTransform?.Replace != null ? new UriTransformSubdocument(uriTransform.Replace) : null;
        }

        private SubscriberEntity Map(SubscriberDocument subscriberDocument)
        {
            var eventEntity = new EventEntity(subscriberDocument.EventName);

            var subscriberEntity = new SubscriberEntity(
                subscriberDocument.SubscriberName,
                eventEntity,
                subscriberDocument.Etag);

            subscriberEntity.AddWebhooks(Map(subscriberDocument.Webhooks, subscriberEntity));

            return subscriberEntity;
        }

        private WebhooksEntity Map(WebhookSubdocument webhookSubdocument, SubscriberEntity subscriberEntity)
        {
            var uriTransformEntity = Map(webhookSubdocument.UriTransform);
            var endpoints = webhookSubdocument.Endpoints.Select(x => Map(x, subscriberEntity));
            return new WebhooksEntity(webhookSubdocument.SelectionRule, endpoints, uriTransformEntity);
        }

        private EndpointEntity Map(EndpointSubdocument endpointSubdocument, SubscriberEntity subscriberEntity)
        {
            var authentication = MapToAuthenticationEntity(endpointSubdocument.Authentication);
            return new EndpointEntity(endpointSubdocument.Uri, authentication, endpointSubdocument.HttpVerb, endpointSubdocument.Selector, subscriberEntity);
        }

        private UriTransformEntity Map(UriTransformSubdocument uriTransformSubdocument)
        {
            if (uriTransformSubdocument?.Replace == null)
            {
                return null;
            }

            return new UriTransformEntity(uriTransformSubdocument.Replace);
        }

        private AuthenticationEntity MapToAuthenticationEntity(AuthenticationSubdocument authentication)
        {
            return authentication switch
            {
                BasicAuthenticationSubdocument doc => new BasicAuthenticationEntity(doc.Username, doc.Password),
                OidcAuthenticationSubdocument doc => new OidcAuthenticationEntity(doc.ClientId, doc.SecretName, doc.Uri, doc.Scopes),
                _ => null
            };
        }

        private AuthenticationSubdocument MapToAuthenticationSubdocument(AuthenticationEntity authenticationEntity)
        {
            return authenticationEntity switch
            {
                BasicAuthenticationEntity ent => new BasicAuthenticationSubdocument
                {
                    Username = ent.Username,
                    Password = ent.Password
                },
                OidcAuthenticationEntity ent => new OidcAuthenticationSubdocument
                {
                    SecretName = ent.ClientSecretKeyName,
                    Scopes = ent.Scopes,
                    ClientId = ent.ClientId,
                    Uri = ent.Uri
                },
                _ => null
            };
        }

        #endregion
    }
}
