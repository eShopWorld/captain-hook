using Eshopworld.Data.CosmosDb;

namespace CaptainHook.Storage.Cosmos.QueryBuilders
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
        /// <returns>A cosmos repository query object</returns>
        CosmosQuery BuildSelectSubscribersListEndpoints(string eventName);

        /// <summary>
        /// Build the query to get all subscribers endpoints
        /// </summary>
        /// <returns>A cosmos repository query object</returns>
        CosmosQuery BuildSelectAllSubscribersEndpoints();
    }
}
