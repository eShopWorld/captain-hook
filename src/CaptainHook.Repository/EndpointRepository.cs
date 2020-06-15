using CaptainHook.Repository.Models;
using Eshopworld.Data.CosmosDb;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainEndpoint = CaptainHook.Domain.Models.Endpoint;
using Subscriber = CaptainHook.Domain.Models.EndpointSubscriber;

namespace CaptainHook.Repository
{
    public class EndpointRepository : IEndpointRepository
    {
        private readonly ICosmosDbRepository _cosmosDbRepository;

        public string CollectionName { get; } = "endpoints";
        //public string GenerateId(Endpoint entity) => Guid.NewGuid().ToString();
        //public string ResolvePartitionKey(Endpoint entity) => $"{entity.EventName}-{entity.SubscriberName}";

        public EndpointRepository(ICosmosDbRepository cosmosDbRepository)
        {
            _cosmosDbRepository = cosmosDbRepository;
            _cosmosDbRepository.UseCollection(CollectionName);
        }

        public async Task<Subscriber> GetEndpointsBySubscriber(string eventName, string subscriberName)
        {
            var partitionKey = $"{eventName}-{subscriberName}";
            var query = new QueryDefinition("select * from c");
            var endpoints = await _cosmosDbRepository.QueryAsync<Endpoint>(new CosmosQuery(query, partitionKey));
            return MapToSubscriber(endpoints);
        }

        private DomainEndpoint MapToDomainEndpoint(Endpoint endpoint)
        {
            return new DomainEndpoint
            {
                
            };
        }

        private Subscriber MapToSubscriber(IEnumerable<Models.Endpoint> endpoints)
        {
            var subscriber = new Subscriber();
            foreach(var endpoint in endpoints)
            {
                var endpoint = MapToDomainEndpoint(endpoint);
                switch(endpoint.WebhookType)
                {
                    case WebhookType.Webhook:
                        subscriber.Webhooks.Endpoints.Add();
                        break;
                    case WebhookType.Callback:
                        subscriber.Webhooks.Endpoints.Add();
                        break;
                    case WebhookType.Dlq:
                        subscriber.Webhooks.Endpoints.Add();
                        break;
                    default:
                        throw Arg
                }

            }
        }

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
