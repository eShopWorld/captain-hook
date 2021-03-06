using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Common.ServiceBus;
using CaptainHook.Common.Telemetry;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.DirectorService.ReaderServiceManagement;
using CaptainHook.Storage.Cosmos;
using Eshopworld.Data.CosmosDb.Extensions;
using Eshopworld.DevOps;
using Eshopworld.Telemetry;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.DirectorService
{
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static async Task Main()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .UseDefaultConfigs()
                    .AddKeyVaultSecrets(new Dictionary<string, string>
                    {
                        {"cm--ai-telemetry--instrumentation", "Telemetry:InstrumentationKey"},
                        {"cm--ai-telemetry--internal", "Telemetry:InternalKey"},
                        {"cm--cosmos-connection--esw-platform", "CosmosDB:ConnectionString"},
                        {"cm--sb-connection--esw-eda", $"{nameof(ServiceBusSettings)}:{nameof(ServiceBusSettings.ConnectionString)}"},
                    }).Build();

                var telemetrySettings = configuration.GetSection("Telemetry").Get<TelemetrySettings>();
                var bb = BigBrother.CreateDefault(telemetrySettings.InstrumentationKey, telemetrySettings.InternalKey);
                bb.UseEventSourceSink().ForExceptions();

                var serviceBusSettings = configuration.GetSection(nameof(ServiceBusSettings)).Get<ServiceBusSettings>();

                //Get configs from the Config Package
                var activationContext = FabricRuntime.GetActivationContext();
                var defaultServicesSettings = ConfigFabricCodePackage(activationContext);

                var builder = new ContainerBuilder();

                builder.RegisterType<SubscriberConfigurationLoader>()
                    .As<ISubscriberConfigurationLoader>()
                    .SingleInstance();

                builder.RegisterInstance(defaultServicesSettings)
                    .SingleInstance();

                builder.RegisterInstance(serviceBusSettings)
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

                builder.RegisterType<SubscriberEntityToConfigurationMapper>()
                    .As<ISubscriberEntityToConfigurationMapper>()
                    .SingleInstance();

                builder.RegisterKeyVaultSecretProvider(configuration);

                builder.RegisterModule<CosmosDbStorageModule>();

                builder.RegisterType<MessageProviderFactory>()
                    .As<IMessageProviderFactory>()
                    .SingleInstance();

                builder.RegisterType<ServiceBusManager>()
                    .As<IServiceBusManager>();

                builder.ConfigureCosmosDb(configuration);

                builder.SetupFullTelemetry(telemetrySettings.InstrumentationKey, telemetrySettings.InternalKey);
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
