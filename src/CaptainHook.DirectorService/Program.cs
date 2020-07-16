using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Common.Telemetry;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.DirectorService.ReaderServiceManagement;
using CaptainHook.Storage.Cosmos;
using Eshopworld.Data.CosmosDb;
using Eshopworld.Data.CosmosDb.Extensions;
using Eshopworld.Telemetry;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.DirectorService
{
    internal static class Program
    {
        private const string CaptainHookConfigSection = "CaptainHook";

        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static async Task Main()
        {
            try
            {
                var appSettings = TempConfigLoader.Load();

                var configurationSettings = new ConfigurationSettings();
                appSettings.Bind(configurationSettings);

                //Get configs from the Config Package
                var activationContext = FabricRuntime.GetActivationContext();
                var defaultServicesSettings = ConfigFabricCodePackage(activationContext);

                var bb = BigBrother.CreateDefault(configurationSettings.InstrumentationKey, configurationSettings.InstrumentationKey);
                bb.UseEventSourceSink().ForExceptions();

                var builder = new ContainerBuilder();

                builder.RegisterType<SubscriberConfigurationLoader>()
                    .As<ISubscriberConfigurationLoader>()
                    .SingleInstance();

                builder.RegisterType<ConfigurationMerger>()
                    .As<IConfigurationMerger>()
                    .SingleInstance();

                builder.RegisterModule<CosmosDbStorageModule>();

                builder.RegisterInstance(configurationSettings)
                       .SingleInstance();

                builder.RegisterInstance(defaultServicesSettings)
                    .SingleInstance();

                builder.RegisterType<FabricClient>()
                    .SingleInstance();

                builder.RegisterType<FabricClientWrapper>()
                    .As<IFabricClientWrapper>()
                    .SingleInstance();

                builder.RegisterType<ReaderServicesManager>()
                    .As<IReaderServicesManager>()
                    .SingleInstance();

                builder.RegisterType<ReaderServiceChangesDetector>()
                    .As<IReaderServiceChangesDetector>()
                    .SingleInstance();

                builder.RegisterModule<KeyVaultModule>();
                builder.RegisterModule<CosmosDbModule>();
                builder.ConfigureCosmosDb(appSettings.GetSection(CaptainHookConfigSection));

                builder.SetupFullTelemetry(configurationSettings.InstrumentationKey);
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
