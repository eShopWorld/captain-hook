using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.Common.Configuration
{
    public sealed class Configuration
    {
        /// <summary>
        /// Get all the configuration settings
        /// </summary>
        public IConfigurationRoot Settings { get; private set; }

        /// <summary>
        /// Get the subscriber configurations
        /// </summary>
        public IDictionary<string, SubscriberConfiguration> SubscriberConfigurations { get; private set; }

        /// <summary>
        /// Get the webhook configurations
        /// </summary>
        public IList<WebhookConfig> WebhookConfigurations { get; private set; }

        private Configuration()
        {
        }

        /// <summary>
        /// Load configuration settings and domain events 
        /// </summary>
        /// <returns>An instance holding the configuration settings and domain events</returns>
        public static Configuration Load()
        {
            var result = new Configuration();
            result.Settings = ConfigurationLoader.Load();
            result.InitSubscribersAndWebhook();

            // result.LoadFromCosmosDb();

            return result;
        }

        private void InitSubscribersAndWebhook()
        {
            // Load and autowire event config
            var values = this.Settings.GetSection("event").GetChildren().ToList();

            this.SubscriberConfigurations = new Dictionary<string, SubscriberConfiguration>(values.Count);
            this.WebhookConfigurations = new List<WebhookConfig>(values.Count);
            var endpointList = new Dictionary<string, WebhookConfig>(values.Count);

            foreach (var configurationSection in values)
            {
                //temp work around until config comes in through the API
                var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();

                foreach (var subscriber in eventHandlerConfig.AllSubscribers)
                {
                    this.SubscriberConfigurations.Add(
                        SubscriberConfiguration.Key(eventHandlerConfig.Type, subscriber.SubscriberName),
                        subscriber);

                    var path = subscriber.WebHookConfigPath;
                    ConfigParser.ParseAuthScheme(subscriber, configurationSection, $"{path}:authenticationconfig");
                    subscriber.EventType = eventHandlerConfig.Type;
                    subscriber.PayloadTransformation = subscriber.DLQMode != null ? PayloadContractTypeEnum.WrapperContract : PayloadContractTypeEnum.Raw;

                    this.WebhookConfigurations.Add(subscriber);
                    ConfigParser.AddEndpoints(subscriber, endpointList, configurationSection, path);

                    if (subscriber.Callback != null)
                    {
                        path = subscriber.CallbackConfigPath;
                        ConfigParser.ParseAuthScheme(subscriber.Callback, configurationSection, $"{path}:authenticationconfig");
                        subscriber.Callback.EventType = eventHandlerConfig.Type;
                        this.WebhookConfigurations.Add(subscriber.Callback);
                        ConfigParser.AddEndpoints(subscriber.Callback, endpointList, configurationSection, path);
                    }
                }
            }
        }
    }
}
