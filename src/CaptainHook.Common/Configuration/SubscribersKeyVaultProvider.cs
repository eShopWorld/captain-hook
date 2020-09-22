using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.Common.Configuration
{
    public class SubscribersKeyVaultProvider : ISubscribersKeyVaultProvider
    {
        private readonly IKeyVaultConfigurationLoader _configurationLoader;

        public SubscribersKeyVaultProvider(IKeyVaultConfigurationLoader configurationLoader)
        {
            _configurationLoader = configurationLoader ?? throw new ArgumentNullException(nameof(configurationLoader));
        }

        public OperationResult<IEnumerable<SubscriberConfiguration>> Load(string keyVaultUri)
        {
            var root = _configurationLoader.Load(keyVaultUri);
            var configurationSections = root.GetSection("event").GetChildren().ToList();

            var subscribers = new List<SubscriberConfiguration>(configurationSections.Count);

            var failures = new List<KeyVaultConfigurationFailure>();

            foreach (var configurationSection in configurationSections)
            {
                //temp work around until config comes in through the API
                var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();

                foreach (var subscriber in eventHandlerConfig.AllSubscribers)
                {
                    try
                    {
                        subscribers.Add(subscriber);

                        var path = subscriber.WebHookConfigPath;
                        ConfigParser.ParseAuthScheme(subscriber, configurationSection, $"{path}:authenticationconfig");
                        subscriber.EventType = eventHandlerConfig.Type;
                        subscriber.PayloadTransformation = subscriber.DLQMode != null ? PayloadContractType.WrapperContract : PayloadContractType.Raw;
                        ConfigParser.AddEndpoints(subscriber, configurationSection, path);

                        if (subscriber.Callback != null)
                        {
                            path = subscriber.CallbackConfigPath;
                            ConfigParser.ParseAuthScheme(subscriber.Callback, configurationSection, $"{path}:authenticationconfig");
                            subscriber.Callback.EventType = eventHandlerConfig.Type;
                            ConfigParser.AddEndpoints(subscriber.Callback, configurationSection, path);
                        }
                    }
                    catch (Exception ex)
                    {
                        failures.Add(new KeyVaultConfigurationFailure(configurationSection.Path, ex));
                    }
                }
            }

            if (failures.Any())
            {
                return new KeyVaultConfigurationError(failures);
            }

            return subscribers;
        }
    }
}