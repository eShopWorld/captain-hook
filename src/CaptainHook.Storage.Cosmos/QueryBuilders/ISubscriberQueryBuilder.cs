using Eshopworld.Data.CosmosDb;

namespace CaptainHook.Storage.Cosmos.QueryBuilders
{
    /// <summary>
    /// Query Builder interface
    /// </summary>
    public interface ISubscriberQueryBuilder
    {
        /// <summary>
        /// Build the query to get a list of subscriber for a given event
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns>A cosmos repository query object</returns>
        CosmosQuery BuildSelectForEventSubscribers(string eventName);

        /// <summary>
        /// Build the query to get all subscribers
        /// </summary>
        /// <returns>A cosmos repository query object</returns>
        CosmosQuery BuildSelectAllSubscribers();

        /// <summary>
        /// Build the query to get a specific subscriber
        /// </summary>
        /// <returns>A cosmos repository query object</returns>
        CosmosQuery BuildSelectSubscriber(string subscriberId, string eventName);
    }
}
