using Eshopworld.Data.CosmosDb;

namespace CaptainHook.Repository.QueryBuilders
{
    /// <summary>
    /// Query Builder interface
    /// </summary>
    public interface ISubscriberQueryBuilder
    {
        /// <summary>
        /// Build the query to get a list of subscriber endpoints
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        CosmosQuery BuildSelectSubscribersListEndpoints(string eventName);
    }
}
