﻿using System;
using System.Reflection;
using System.Text;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CaptainHook.Common.ServiceModels
{
    public class EventReaderInitData
    {
        public string EventType { get; set; }
        public string SubscriberName { get; set; }
        public SubscriberDlqMode? DlqMode { get; set; }
        public int MaxDeliveryCount { get; set; }

        /// <summary>
        /// source subscription for DLQ receiver
        /// </summary>
        public string SourceSubscription { get; set; }

        public SubscriberConfiguration SubscriberConfiguration { get; set; }

        public string SubscriptionName => DlqMode != null ? SourceSubscription : SubscriberName;

        public TimeSpan? HeartBeatInterval { get; set; }

        private static readonly JsonIgnoreAttributeIgnorerContractResolver JsonIgnoreAttributeIgnorerContractResolver = new JsonIgnoreAttributeIgnorerContractResolver();

        private static readonly AuthenticationConfigConverter AuthenticationConfigConverter = new AuthenticationConfigConverter();

        public static EventReaderInitData FromSubscriberConfiguration(SubscriberConfiguration subscriberConfiguration)
        {
            return new EventReaderInitData
            {
                SubscriberConfiguration = subscriberConfiguration,
                SubscriberName = subscriberConfiguration.SubscriberName,
                EventType = subscriberConfiguration.EventType,
                DlqMode = subscriberConfiguration.DLQMode,
                SourceSubscription = subscriberConfiguration.DLQMode != null ? subscriberConfiguration.SourceSubscriptionName : null,
                HeartBeatInterval = TimeSpan.TryParse(subscriberConfiguration.HeartBeatInterval, out var heartBeatInterval) ? heartBeatInterval : (TimeSpan?)null,
                MaxDeliveryCount = subscriberConfiguration.MaxDeliveryCount
            };
        }

        public byte[] ToByteArray()
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = JsonIgnoreAttributeIgnorerContractResolver;
            var stringValue = JsonConvert.SerializeObject(this, settings);
            var byteArray = Encoding.UTF8.GetBytes(stringValue);
            return byteArray;
        }

        public static EventReaderInitData FromByteArray(byte[] buffer)
        {
            var content = Encoding.UTF8.GetString(buffer);
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = JsonIgnoreAttributeIgnorerContractResolver;
            settings.Converters.Add(AuthenticationConfigConverter);
            return JsonConvert.DeserializeObject<EventReaderInitData>(content, settings);
        }
    }

    internal sealed class JsonIgnoreAttributeIgnorerContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.Ignored = false;
            return property;
        }
    }
}
