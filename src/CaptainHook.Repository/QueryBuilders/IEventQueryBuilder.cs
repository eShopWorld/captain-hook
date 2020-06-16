using Eshopworld.Data.CosmosDb;

namespace CaptainHook.Repository.QueryBuilders
{
    /// <summary>
    /// Query Builder interface
    /// </summary>
    public interface IEventQueryBuilder
    {
        CosmosQuery BuildSelectSubscriberEndpoints(string eventName, string subscriberName);
        
        CosmosQuery BuildSelectSubscribersEndpoints(string eventName);
    }
}
