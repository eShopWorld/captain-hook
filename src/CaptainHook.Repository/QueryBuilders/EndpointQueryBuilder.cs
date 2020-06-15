using Eshopworld.Data.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace CaptainHook.Repository.QueryBuilders
{
    public class EndpointQueryBuilder: IEndpointQueryBuilder
    {
        public CosmosQuery BuildSelectSubscriberEndpoints(string eventName, string subscriberName)
        {
            var partitionKey = $"{eventName}-{subscriberName}";
            var query = new QueryDefinition("select * from c");
            return new CosmosQuery(query, partitionKey);
        }

        public CosmosQuery BuildSelectSubscribersEndpoints(string eventName)
        {
            var query = new QueryDefinition("select * from c where name = @name")
                .WithParameter("@name", eventName);
            return new CosmosQuery(query);
        }
    }
}
