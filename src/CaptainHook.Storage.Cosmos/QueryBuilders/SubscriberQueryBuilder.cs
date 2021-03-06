﻿using CaptainHook.Storage.Cosmos.Models;
using Eshopworld.Data.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace CaptainHook.Storage.Cosmos.QueryBuilders
{
    public class SubscriberQueryBuilder: ISubscriberQueryBuilder
    {
        public CosmosQuery BuildSelectForEventSubscribers(string eventName)
        {
            var partitionKey = SubscriberDocument.GetPartitionKey(eventName);

            var query = new QueryDefinition("select * from c where c.eventName = @eventName and c.type = @documentType")
                .WithParameter("@eventName", eventName)
                .WithParameter("@documentType", SubscriberDocument.Type);

            return new CosmosQuery(query, partitionKey);
        }

        public CosmosQuery BuildSelectAllSubscribers()
        {
            var query = new QueryDefinition("select * from c where c.type = @documentType")
                .WithParameter("@documentType", SubscriberDocument.Type);

            return new CosmosQuery(query);
        }

        public CosmosQuery BuildSelectSubscriber(string subscriberId, string eventName)
        {
            var query = new QueryDefinition(@"select * from c where c.id = @id")
                .WithParameter("@id", subscriberId.ToString());

            var partitionKey = SubscriberDocument.GetPartitionKey(eventName);

            return new CosmosQuery(query, partitionKey);
        }
    }
}
