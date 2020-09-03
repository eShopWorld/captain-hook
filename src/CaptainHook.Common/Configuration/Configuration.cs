using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace CaptainHook.Common.Configuration
{
    public interface ISubscribersKeyVaultProvider
    {
        IDictionary<string, SubscriberConfiguration> Load(string keyVaultUri);
    }

    public class SubscribersKeyVaultProvider : ISubscribersKeyVaultProvider
    {
        public IDictionary<string, SubscriberConfiguration> Load(string keyVaultUri)
        {
            var root = new ConfigurationBuilder()
               .AddAzureKeyVault(
                   keyVaultUri,
                   new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)),
                   new DefaultKeyVaultSecretManager())
               //.AddEnvironmentVariables()
               .Build();

            // Load and autowire event config
            var configurationSections = root.GetSection("event").GetChildren().ToList();

            var subscribers = new Dictionary<string, SubscriberConfiguration>(configurationSections.Count);

            foreach (var configurationSection in configurationSections)
            {
                //temp work around until config comes in through the API
                var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();

                foreach (var subscriber in eventHandlerConfig.AllSubscribers)
                {
                    subscribers.Add(
                        SubscriberConfiguration.Key(eventHandlerConfig.Type, subscriber.SubscriberName),
                        subscriber);

                    var path = subscriber.WebHookConfigPath;
                    ConfigParser.ParseAuthScheme(subscriber, configurationSection, $"{path}:authenticationconfig");
                    subscriber.EventType = eventHandlerConfig.Type;
                    subscriber.PayloadTransformation = subscriber.DLQMode != null ? PayloadContractTypeEnum.WrapperContract : PayloadContractTypeEnum.Raw;
                    ConfigParser.AddEndpoints(subscriber, configurationSection, path);

                    if (subscriber.Callback != null)
                    {
                        path = subscriber.CallbackConfigPath;
                        ConfigParser.ParseAuthScheme(subscriber.Callback, configurationSection, $"{path}:authenticationconfig");
                        subscriber.Callback.EventType = eventHandlerConfig.Type;
                        ConfigParser.AddEndpoints(subscriber.Callback, configurationSection, path);
                    }
                }
            }

            return subscribers;
        }
    }
}

    //public sealed class Configuration
    //{
    //    /// <summary>
    //    /// Get all the configuration settings
    //    /// </summary>
    //    public IConfigurationRoot Settings { get; private set; }

    //    /// <summary>
    //    /// Get the subscriber configurations
    //    /// </summary>
    //    public IDictionary<string, SubscriberConfiguration> SubscriberConfigurations { get; private set; }

    //    private Configuration()
    //    {
    //    }

    //    /// <summary>
    //    /// Load configuration settings and domain events 
    //    /// </summary>
    //    /// <param name="keyVaultUri"></param>
    //    /// <returns>An instance holding the configuration settings and domain events</returns>
    //    public static Configuration Load(string keyVaultUri)
    //    {
    //        var result = new Configuration();
    //        result.Settings = ConfigurationLoader.Load(keyVaultUri);
    //        result.InitSubscribersAndWebhook();

    //        // result.LoadFromCosmosDb();

    //        return result;
    //    }

    //    private void InitSubscribersAndWebhook()
    //    {
    //        // Load and autowire event config
    //        var values = this.Settings.GetSection("event").GetChildren().ToList();

    //        this.SubscriberConfigurations = new Dictionary<string, SubscriberConfiguration>(values.Count);

    //        foreach (var configurationSection in values)
    //        {
    //            //temp work around until config comes in through the API
    //            var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();

    //            foreach (var subscriber in eventHandlerConfig.AllSubscribers)
    //            {
    //                this.SubscriberConfigurations.Add(
    //                    SubscriberConfiguration.Key(eventHandlerConfig.Type, subscriber.SubscriberName),
    //                    subscriber);

    //                var path = subscriber.WebHookConfigPath;
    //                ConfigParser.ParseAuthScheme(subscriber, configurationSection, $"{path}:authenticationconfig");
    //                subscriber.EventType = eventHandlerConfig.Type;
    //                subscriber.PayloadTransformation = subscriber.DLQMode != null ? PayloadContractTypeEnum.WrapperContract : PayloadContractTypeEnum.Raw;

    //                ConfigParser.AddEndpoints(subscriber, configurationSection, path);

    //                if (subscriber.Callback != null)
    //                {
    //                    path = subscriber.CallbackConfigPath;
    //                    ConfigParser.ParseAuthScheme(subscriber.Callback, configurationSection, $"{path}:authenticationconfig");
    //                    subscriber.Callback.EventType = eventHandlerConfig.Type;
    //                    ConfigParser.AddEndpoints(subscriber.Callback, configurationSection, path);
    //                }
    //            }
    //        }
    //    }
//    }
//}
