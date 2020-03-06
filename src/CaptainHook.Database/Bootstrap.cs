using Autofac;
using CaptainHook.Database.CosmosDB;

namespace CaptainHook.Database
{
    /// <summary>
    /// Module for dependencies in the repository
    /// </summary>
    public class Bootstrap : Module
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CosmosDbClientFactory>()
                .As<ICosmosDbClientFactory>()
                .SingleInstance();

            builder.RegisterType<CosmosDbRepository>().As<ICosmosDbRepository>();
        }
    }
}
