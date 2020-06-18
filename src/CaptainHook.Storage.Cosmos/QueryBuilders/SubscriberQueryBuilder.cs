using CaptainHook.Repository.Models;
using Eshopworld.Data.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace CaptainHook.Repository.QueryBuilders
{
    public class SubscriberQueryBuilder: ISubscriberQueryBuilder
    {
        public CosmosQuery BuildSelectSubscribersListEndpoints(string eventName)
        {
            var partitionKey = EndpointDocument.GetPartitionKey(eventName);

            var query = new QueryDefinition("select * from c where eventName = @eventName and documentType = @documentType")
                .WithParameter("@eventName", eventName)
                .WithParameter("@documentType", EndpointDocument.Type);

            return new CosmosQuery(query, partitionKey);
        }
    }
}
