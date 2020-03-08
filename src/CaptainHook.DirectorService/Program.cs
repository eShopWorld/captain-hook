using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using CaptainHook.Database.Setup;
using Eshopworld.Telemetry;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace CaptainHook.DirectorService
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static async Task Main()
        {
            try
            {
                var kvUri = Environment.GetEnvironmentVariable(ConfigurationSettings.KeyVaultUriEnvVariable);

                var config = new ConfigurationBuilder()
                    .AddAzureKeyVault(
                        kvUri,
                        new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)),
                        new DefaultKeyVaultSecretManager())
                    .AddEnvironmentVariables("EDA_")
                    .Build();

                var settings = new ConfigurationSettings();
                config.Bind(settings);
                
                //Get configs from the Config Package
                var activationContext = FabricRuntime.GetActivationContext();
                var defaultServicesSettings = ConfigFabricCodePackage(activationContext);

                var bb = new BigBrother(settings.InstrumentationKey, settings.InstrumentationKey);
                bb.UseEventSourceSink().ForExceptions();

                var builder = new ContainerBuilder();

                builder.RegisterInstance(config.GetSection("event").Get<IEnumerable<EventHandlerConfig>>());

                // CosmosDB support is under development
                if (false)
                {
                    builder.ConfigureCosmosDb(config);
                }

                builder.RegisterInstance(settings)
                       .SingleInstance();

                builder.RegisterInstance(defaultServicesSettings)
                    .SingleInstance();

                builder.RegisterType<FabricClient>().SingleInstance();

                //todo cosmos config and rules stuff - come back to this later
                // - we need to ensure rules which are in the db are created
                // - rules which are updated are updated in the handlers and dispatchers
                // - rules which are deleted can only be soft deleted - cosmos change feed does not support hard deletes
                //var cosmosClient = new CosmosClient(settings.CosmosConnectionString);
                //builder.RegisterInstance(cosmosClient);

                //var database = (await cosmosClient.Databases.CreateDatabaseIfNotExistsAsync("captain-hook", 400)).Database;
                //builder.RegisterInstance(database);

                //var ruleContainer = (await database.Containers.CreateContainerIfNotExistsAsync(nameof(RoutingRule), RoutingRule.PartitionKeyPath)).Container;
                //builder.RegisterInstance(ruleContainer).SingleInstance();

                builder.SetupFullTelemetry(settings.InstrumentationKey);
                builder.RegisterStatefulService<DirectorService>(ServiceNaming.DirectorServiceType);

                using (builder.Build())
                {
                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, ServiceNaming.DirectorServiceShortName);

                    await Task.Delay(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                BigBrother.Write(e);
                throw;
            }
        }

        /// <summary>
        /// Parsing the config package configs for this services
        /// </summary>
        /// <param name="activationContext"></param>
        /// <returns></returns>
        private static DefaultServiceSettings ConfigFabricCodePackage(ICodePackageActivationContext activationContext)
        {
            var configurationPackage = activationContext.GetConfigurationPackageObject("Config");
            var section = configurationPackage.Settings.Sections[nameof(Constants.CaptainHookApplication.DefaultServiceConfig)];

            return new DefaultServiceSettings
            {
                DefaultMinReplicaSetSize = GetValueAsInt(Constants.CaptainHookApplication.DefaultServiceConfig.DefaultMinReplicaSetSize, section),
                DefaultPartitionCount = GetValueAsInt(Constants.CaptainHookApplication.DefaultServiceConfig.DefaultPartitionCount, section),
                DefaultTargetReplicaSetSize = GetValueAsInt(Constants.CaptainHookApplication.DefaultServiceConfig.TargetReplicaSetSize, section),
                DefaultPlacementConstraints = section.Parameters[Constants.CaptainHookApplication.DefaultServiceConfig.DefaultPlacementConstraints].Value
            };
        }

        /// <summary>
        /// Simple helper to parse the ConfigurationSection from ServiceFabric Manifests for particular values.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        private static int GetValueAsInt(string key, System.Fabric.Description.ConfigurationSection section)
        {
            var result = int.TryParse(section.Parameters[key].Value, out var value);

            if (!result)
            {
                throw new Exception($"Code package could not be parsed for value {key}");
            }

            return value;
        }
    }
}
