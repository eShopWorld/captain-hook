using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.Database.CosmosDB
{
    public class CosmosDbCollectionSettings
    {
        public string CollectionName { get; set; }
        public string PartitionKey { get; set; }
        public string[] UniqueKeys { get; set; }

        public CosmosDbCollectionSettings(): this (null) { } // default constructor needed
        public CosmosDbCollectionSettings (string collectionName) 
        {
            CollectionName = collectionName;
        }
    }

    public class CosmosDbConfiguration
    {
        public string DatabaseEndpoint { get; set; }
        public string DatabaseKey { get; set; }

        public int Throughput {  get; set; } = 400;

        /// <summary>
        /// Defines list of databases with their settings.
        /// The key represents the database name.
        /// </summary>
        public IDictionary<string, CosmosDbCollectionSettings[]> Databases{ get; set; }

        public bool TryGetDefaults (out string databaseId, out string collectionName)
        {
            var defaults = Databases.FirstOrDefault();
            if (defaults.Key != null && defaults.Value != null && defaults.Value.Any())
            {
                databaseId = defaults.Key;
                collectionName = defaults.Value[0].CollectionName;
                return ! string.IsNullOrWhiteSpace(databaseId) && ! string.IsNullOrWhiteSpace (collectionName);
            }

            databaseId = null;
            collectionName = null;
            return false;
        }
    }
}