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
        public string GenerateName(SubscriberConfiguration configuration)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var asByteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(configuration));
            var hashBytes = md5.ComputeHash(asByteArray);
            var suffix = hashBytes.ToBase62();
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(configuration.EventType, configuration.SubscriberName, configuration.DLQMode.HasValue);

            return $"{readerServiceNameUri}-{suffix}";
        }
    }
}