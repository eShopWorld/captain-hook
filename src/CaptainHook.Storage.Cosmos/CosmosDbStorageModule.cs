using Autofac;
using CaptainHook.Domain.Repositories;
using CaptainHook.Storage.Cosmos.QueryBuilders;
using Eshopworld.Data.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace CaptainHook.Storage.Cosmos
{
    /// <summary>
    /// Module for dependencies in the repository
    /// </summary>
    public class CosmosDbStorageModule : Module
    {
        private static readonly CosmosClientOptions CosmosClientOptions = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };

        /// <summary>
        /// Load module dependencies
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SubscriberQueryBuilder>()
                .As<ISubscriberQueryBuilder>()
                .SingleInstance();

            builder.RegisterType<SubscriberRepository>()
                .As<ISubscriberRepository>()
                .SingleInstance();
            
            builder.RegisterModule<CosmosDbModule>();

            builder.RegisterInstance(CosmosClientOptions);
        }
    }
}
