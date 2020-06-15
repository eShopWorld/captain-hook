using CaptainHook.Domain.Models;
using CaptainHook.Repository.Models;
using Eshopworld.Data.CosmosDb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DbEndpoint = CaptainHook.Repository.Models.Endpoint;
using DomainEndpoint = CaptainHook.Domain.Models.Endpoint;
using DomainAuthentication = CaptainHook.Domain.Models.Authentication;
using CaptainHook.Repository.QueryBuilders;
using System.Linq;

namespace CaptainHook.Repository
{
    public class EndpointRepository : IEndpointRepository
    {
        private readonly ICosmosDbRepository _cosmosDbRepository;
        private readonly IEndpointQueryBuilder _endpointQueryBuilder;

        public string CollectionName { get; } = "endpoints";
        //public string GenerateId(Endpoint entity) => Guid.NewGuid().ToString();
        //public string ResolvePartitionKey(Endpoint entity) => $"{entity.EventName}-{entity.SubscriberName}";

        public EndpointRepository(ICosmosDbRepository cosmosDbRepository, IEndpointQueryBuilder endpointQueryBuilder)
        {
            _cosmosDbRepository = cosmosDbRepository ?? throw new ArgumentNullException(nameof(cosmosDbRepository));
            _endpointQueryBuilder = endpointQueryBuilder ?? throw new ArgumentNullException(nameof(endpointQueryBuilder));

            _cosmosDbRepository.UseCollection(CollectionName);
        }

        public Task<Subscriber> GetSubscriberAsync(string eventName, string subscriberName)
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

        public Task<IEnumerable<Subscriber>> GetSubscribersAsync(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            return GetSubscribersInternalAsync(eventName);
        }

        #region Private methods

        private async Task<IEnumerable<Subscriber>> GetSubscribersInternalAsync(string eventName)
        {
            var query = _endpointQueryBuilder.BuildSelectSubscribersEndpoints(eventName);
            var endpoints = await _cosmosDbRepository.QueryAsync<DbEndpoint>(query);

            return endpoints
                .GroupBy(x => x.SubscriberName)
                .Select(x => Map(x));
        }

        private async Task<Subscriber> GetSubscriberInternalAsync(string eventName, string subscriberName)
        {
            var query = _endpointQueryBuilder.BuildSelectSubscriberEndpoints(eventName, subscriberName);
            var endpoints = await _cosmosDbRepository.QueryAsync<DbEndpoint>(query);
            return Map(endpoints);
        }

        private DomainEndpoint Map(DbEndpoint endpoint)
        {
            return new DomainEndpoint
            {
                HttpVerb = endpoint.HttpVerb,
                Uri = endpoint.Uri,
                Authentication = new DomainAuthentication
                {
                    Scopes = endpoint.Authentication.Scopes,
                    ClientSecret = endpoint.Authentication.ClientSecret,
                    ClientId = endpoint.Authentication.ClientId,
                    Uri = endpoint.Authentication.Uri,
                    Type = endpoint.Authentication.Type
                },
                Selector = endpoint.Selector
            };
        }

        private Subscriber Map(IEnumerable<DbEndpoint> endpoints)
        {
            var subscriber = new Subscriber
            {
                Name = endpoints.First().SubscriberName
            };

            foreach (var endpoint in endpoints)
            {
                var domainEndpoint = Map(endpoint);
                domainEndpoint.Subscriber = subscriber;
                switch (endpoint.WebhookType)
                {
                    case WebhookType.Webhook:
                        subscriber.Webhooks.Endpoints.Add(domainEndpoint);
                        break;
                    case WebhookType.Callback:
                        subscriber.Webhooks.Endpoints.Add(domainEndpoint);
                        break;
                    case WebhookType.Dlq:
                        subscriber.Webhooks.Endpoints.Add(domainEndpoint);
                        break;
                    default:
                        throw new ArgumentException("Invalid webhook type for endpoint");
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
