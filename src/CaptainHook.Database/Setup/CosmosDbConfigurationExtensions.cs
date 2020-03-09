using System.Diagnostics.CodeAnalysis;
using Autofac;
using CaptainHook.Database.CosmosDB;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.Database.Setup
{
    /// <summary>
    /// Extension class to wrap configuration of CosmosDB
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class CosmosDbConfigurationExtensions
    {
        /// <summary>
        /// Configures Cosmos DB from existing config store.
        /// </summary>
        public static ContainerBuilder ConfigureCosmosDb(this ContainerBuilder builder, IConfiguration configuration)
        {
            var cosmosDbConfiguration = configuration.GetSection("DbConfiguration").Get<CosmosDbConfiguration>() ?? new CosmosDbConfiguration();
            var (dbEndpoint, dbKey) = DbSettingsParser.GetDbSettings(configuration);
            cosmosDbConfiguration.DatabaseEndpoint = dbEndpoint;
            cosmosDbConfiguration.DatabaseKey = dbKey;

            builder.RegisterInstance(cosmosDbConfiguration);

            return builder;
        }
    }
}