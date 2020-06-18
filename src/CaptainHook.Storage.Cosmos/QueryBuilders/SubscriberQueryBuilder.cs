using CaptainHook.Repository.Models;
using Eshopworld.Data.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace CaptainHook.Repository.QueryBuilders
{
    public class SubscriberQueryBuilder: ISubscriberQueryBuilder
    {
        public CosmosQuery BuildSelectSubscriberEndpoints(string eventName, string subscriberName)
        {
            var query = new QueryDefinition("select * from c where eventName = @eventName and subscriberName = @subscriberName and documentType = @documentType")
                .WithParameter("@eventName", eventName)
                .WithParameter("@subscriberName", subscriberName)
                .WithParameter("@documentType", EndpointDocument.Type);

            return new CosmosQuery(query, EndpointDocument.GetPartitionKey(eventName));
        }

        public CosmosQuery BuildSelectSubscribersListEndpoints(string eventName)
        {
            var query = new QueryDefinition("select * from c where eventName = @eventName and documentType = @documentType")
                .WithParameter("@eventName", eventName)
                .WithParameter("@documentType", EndpointDocument.Type);

            return new CosmosQuery(query);
        }
    }
}
