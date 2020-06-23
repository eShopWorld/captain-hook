using CaptainHook.Storage.Cosmos.Models;
using Eshopworld.Data.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace CaptainHook.Storage.Cosmos.QueryBuilders
{
    public class SubscriberQueryBuilder: ISubscriberQueryBuilder
    {
        public CosmosQuery BuildSelectSubscribersListEndpoints(string eventName)
        {
            var partitionKey = EndpointDocument.GetPartitionKey(eventName);

            var query = new QueryDefinition("select * from c where c.eventName = @eventName and c.type = @documentType")
                .WithParameter("@eventName", eventName)
                .WithParameter("@documentType", EndpointDocument.Type);

            return new CosmosQuery(query, partitionKey);
        }

        public CosmosQuery BuildSelectAllSubscribersEndpoints()
        {
            var query = new QueryDefinition("select * from c where c.type = @documentType")
                .WithParameter("@documentType", EndpointDocument.Type);

            return new CosmosQuery(query);
        }
    }
}
