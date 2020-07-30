using System;
using System.Security.Cryptography;
using System.Text;
using Base62;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.ReaderServiceManagement
{

    /// <summary>
    /// Defines reader service names based on given configuration
    /// </summary>
    public readonly struct DesiredReaderDefinition
    {
        /// <summary>
        /// Reader service configuration (subscriber)
        /// </summary>
        public SubscriberConfiguration SubscriberConfig { get; }

        /// <summary>
        /// Bare reader service name, without suffix (non-versioned)
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Defines full reader service name (versioned)
        /// </summary>
        public string ServiceNameWithSuffix { get; }

        public bool IsValid => ServiceName != null && SubscriberConfig != null && ServiceNameWithSuffix != null;

        public DesiredReaderDefinition (SubscriberConfiguration subscriberConfig)
        {
            SubscriberConfig = subscriberConfig;
            ServiceName = ServiceNaming.EventReaderServiceFullUri (subscriberConfig.EventType, subscriberConfig.SubscriberName, subscriberConfig.DLQMode.HasValue);
            ServiceNameWithSuffix = $"{ServiceName}-{GetEncodedHash (subscriberConfig)}";
        }

        public bool IsTheSameService (ExistingReaderDefinition reader)
        {
            return IsValid 
                   && reader.IsValid
                   && ServiceName.Equals (reader.ServiceName, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsUnchanged (ExistingReaderDefinition reader)
        {
            return IsValid 
                   && reader.IsValid
                   && IsTheSameService (reader) 
                   && ServiceNameWithSuffix.Equals (reader.ServiceNameWithSuffix);
        }

        private static string GetEncodedHash (SubscriberConfiguration configuration)
        {
            using var md5 = new MD5CryptoServiceProvider ();
            var bytes = Encoding.UTF8.GetBytes (JsonConvert.SerializeObject (configuration, Formatting.None));
            var hash = md5.ComputeHash (bytes);
            var encoded = hash.ToBase62 ();
            return encoded.PadRight (22, '0');
        }
    }
}