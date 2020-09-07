using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using Eshopworld.Core;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.Common.Configuration
{
    public class SubscribersKeyVaultProvider : ISubscribersKeyVaultProvider
    {
        private readonly IKeyVaultConfigurationLoader _configurationLoader;
        private readonly IBigBrother _bigBrother;

        public SubscribersKeyVaultProvider(IKeyVaultConfigurationLoader configurationLoader, IBigBrother bigBrother)
        {
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
            _configurationLoader = configurationLoader ?? throw new ArgumentNullException(nameof(configurationLoader));
        }

        public OperationResult<IDictionary<string, SubscriberConfiguration>> Load(string keyVaultUri)
        {
            var configurationSections = _configurationLoader.Load(keyVaultUri);
            var subscribers = new Dictionary<string, SubscriberConfiguration>(configurationSections.Count());

            var failures = new List<KeyVaultConfigurationFailure>();

            foreach (var configurationSection in configurationSections)
            {
                //temp work around until config comes in through the API
                var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();

                foreach (var subscriber in eventHandlerConfig.AllSubscribers)
                {
                    try
                    {
                        var subscriberKey = SubscriberConfiguration.Key(eventHandlerConfig.Type, subscriber.SubscriberName);
                        subscribers.Add(subscriberKey, subscriber);

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
                    catch (Exception ex)
                    {
                        var failure = new KeyVaultConfigurationFailure(configurationSection.Path, ex);
                        failures.Add(failure);

                        //var exceptionEvent = ex.ToExceptionEvent<FailedKeyVaultConfigurationEvent>();
                        //exceptionEvent.ConfigKey = configurationSection.Key;
                        //exceptionEvent.SubscriberKey = subscriberKey;

                        //_bigBrother.Publish(exceptionEvent);
                        //throw;
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