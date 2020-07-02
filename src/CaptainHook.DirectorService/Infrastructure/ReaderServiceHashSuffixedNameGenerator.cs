using System.Security.Cryptography;
using System.Text;
using Base62;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.Infrastructure
{
    internal class ReaderServiceHashSuffixedNameGenerator
    {
        public static string GenerateName(SubscriberConfiguration configuration)
        {
            var md5 = new MD5CryptoServiceProvider();
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(configuration));
            var hash = md5.ComputeHash(bytes);
            var suffix = hash.ToBase62();
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(configuration.EventType, configuration.SubscriberName, configuration.DLQMode.HasValue);
            return $"{readerServiceNameUri}-{suffix}";
        }
    }

    internal class HashCalculator
    {
        public static string GetEncodedHash(SubscriberConfiguration configuration)
        {
            var md5 = new MD5CryptoServiceProvider();
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(configuration));
            var hash = md5.ComputeHash(bytes);
            var encoded = hash.ToBase62();
            return encoded;
        }
    }
}