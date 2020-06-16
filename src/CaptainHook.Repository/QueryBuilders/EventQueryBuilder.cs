using Eshopworld.Data.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace CaptainHook.Repository.QueryBuilders
{
    public class EventQueryBuilder: IEventQueryBuilder
    {
        public CosmosQuery BuildSelectSubscriberEndpoints(string eventName, string subscriberName)
        {
            var partitionKey = $"{eventName}-{subscriberName}";
            var query = new QueryDefinition("select * from c where eventName = @eventName and subscriberName = @subscriberName")
                .WithParameter("@eventName", eventName)
                .WithParameter("@subscriberName", subscriberName);
            return new CosmosQuery(query, partitionKey);
        }

        public CosmosQuery BuildSelectSubscribersEndpoints(string eventName)
        {
            var query = new QueryDefinition("select * from c where eventName = @eventName")
                .WithParameter("@eventName", eventName);
            return new CosmosQuery(query);
        }
    }
}
