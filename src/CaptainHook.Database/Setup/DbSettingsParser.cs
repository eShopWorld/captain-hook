using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.Database.Setup
{
    /// <summary>
    /// Helper to encapsulate logic around fetching DB Connection string settings
    /// <remarks>
    /// Will throw a <see cref="CosmosDbConfigurationException"/> if cannot correctly parse connection string
    /// </remarks>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class DbSettingsParser
    {
        private const string ConnectionStringKey = "CosmosDB:Tooling:ConnectionString";
        private const string EndPointKey = "AccountEndpoint";
        private const string AccountKey = "AccountKey";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <exception cref="CosmosDbConfigurationException"></exception>
        /// <returns></returns>
        public static (string dbEndpoint, string dbKey) GetDbSettings(IConfiguration configuration)
        {
            var connectionString = configuration[ConnectionStringKey];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new CosmosDbConfigurationException("Missing connection string");
            }

            var connectionStringBuilder = new DbConnectionStringBuilder { ConnectionString = connectionString };

            var dbEndpoint = GetDbEndpoint(connectionStringBuilder);
            var databaseKey = GetDatabaseKey(connectionStringBuilder);

            return (dbEndpoint: dbEndpoint, dbKey: databaseKey);
        }

        private static string GetDatabaseKey(DbConnectionStringBuilder connectionStringBuilder)
        {
            if (!connectionStringBuilder.ContainsKey(AccountKey))
            {
                throw new CosmosDbConfigurationException("Missing DB account key");
            }

            var databaseKey = connectionStringBuilder[AccountKey]?.ToString();
            if (string.IsNullOrEmpty(databaseKey))
            {
                throw new CosmosDbConfigurationException("Missing DB account key");
            }

            return databaseKey;
        }

        private static string GetDbEndpoint(DbConnectionStringBuilder connectionStringBuilder)
        {
            if (!connectionStringBuilder.ContainsKey(EndPointKey))
            {
                throw new CosmosDbConfigurationException("Missing DB endpoint");
            }

            var dbEndpoint = connectionStringBuilder[EndPointKey]?.ToString();
            if (string.IsNullOrEmpty(dbEndpoint))
            {
                throw new CosmosDbConfigurationException("Missing DB endpoint");
            }

            return dbEndpoint;
        }
    }
}