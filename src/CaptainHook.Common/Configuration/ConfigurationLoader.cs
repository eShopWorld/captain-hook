using Eshopworld.Core;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.Common.Configuration
{
    public class ConfigurationLoader : IConfigurationLoader
    {
        private readonly IBigBrother _bigBrother;

        public Configuration Configuration { get; }

        public ConfigurationLoader(IBigBrother bigBrother)
        {
            _bigBrother = bigBrother;
        }
        /// <summary>
        /// Loads configuration from Environment and KeyVault
        /// </summary>
        /// <returns>All application configuration properties</returns>
        public Configuration Load(string keyVaultUri)
        {
            if (string.IsNullOrWhiteSpace(keyVaultUri))
            {
                throw new ArgumentNullException(nameof(keyVaultUri));
            }

            var configurationRoot = new ConfigurationBuilder()
                .AddAzureKeyVault(
                    keyVaultUri,
                    new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)),
                    new DefaultKeyVaultSecretManager())
                .AddEnvironmentVariables()
                .Build();

            var subscriberConfigurations = InitSubscribersAndWebhook(configurationRoot);

            return new Configuration(configurationRoot, subscriberConfigurations);
        }

        private Dictionary<string, SubscriberConfiguration> InitSubscribersAndWebhook(IConfiguration configurationRoot)
        {
            try
            {
                // Load and autowire event config
                var values = configurationRoot.GetSection("event").GetChildren().ToList();

                var subscriberConfigurations = new Dictionary<string, SubscriberConfiguration>(values.Count);
                var endpointList = new Dictionary<string, WebhookConfig>(values.Count);

                foreach (var configurationSection in values)
                {
                    //temp work around until config comes in through the API
                    var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();

                    foreach (var subscriber in eventHandlerConfig.AllSubscribers)
                    {
                        subscriberConfigurations.Add(
                            SubscriberConfiguration.Key(eventHandlerConfig.Type, subscriber.SubscriberName),
                            subscriber);

                        var path = subscriber.WebHookConfigPath;
                        ConfigParser.ParseAuthScheme(subscriber, configurationSection, $"{path}:authenticationconfig");
                        subscriber.EventType = eventHandlerConfig.Type;
                        subscriber.PayloadTransformation = subscriber.DLQMode != null ? PayloadContractTypeEnum.WrapperContract : PayloadContractTypeEnum.Raw;

                        ConfigParser.AddEndpoints(subscriber, endpointList, configurationSection, path);

                        if (subscriber.Callback != null)
                        {
                            path = subscriber.CallbackConfigPath;
                            ConfigParser.ParseAuthScheme(subscriber.Callback, configurationSection, $"{path}:authenticationconfig");
                            subscriber.Callback.EventType = eventHandlerConfig.Type;
                            ConfigParser.AddEndpoints(subscriber.Callback, endpointList, configurationSection, path);
                        }
                    }
                }
                return subscriberConfigurations;
            }
            catch(Exception exception)
            {
                _bigBrother.Publish(new ConfigurationParserError { });
            }

        }
    }

    public class Configuration
    {
        /// <summary>
        /// Get all the configuration settings
        /// </summary>
        public IConfigurationRoot Settings { get; }

        /// <summary>
        /// Get the subscriber configurations
        /// </summary>
        public IDictionary<string, SubscriberConfiguration> SubscriberConfigurations { get; }

        public Configuration(IConfigurationRoot settings, IDictionary<string, SubscriberConfiguration> subscriberConfigurations)
        {
            Settings = settings;
            SubscriberConfigurations = subscriberConfigurations;
        }
    }
}