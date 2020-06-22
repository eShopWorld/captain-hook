using Autofac;
using CaptainHook.Domain.Repositories;
using CaptainHook.Storage.Cosmos.QueryBuilders;

namespace CaptainHook.Storage.Cosmos
{
    /// <summary>
    /// Module for dependencies in the repository
    /// </summary>
    public class CosmosDbStorageModule : Module
    {
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
        }
    }
}
