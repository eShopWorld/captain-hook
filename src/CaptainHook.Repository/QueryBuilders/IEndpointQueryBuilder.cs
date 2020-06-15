using Eshopworld.Data.CosmosDb;

namespace CaptainHook.Repository.QueryBuilders
{
    public interface IEndpointQueryBuilder
    {
        CosmosQuery BuildSelectSubscriberEndpoints(string eventName, string subscriberName);
        
        CosmosQuery BuildSelectSubscribersEndpoints(string eventName);
    }
}
