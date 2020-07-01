using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Base62;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.Infrastructure
{
    internal class ReaderServiceHashSuffixedNameGenerator
    {
        public string GenerateNewName(SubscriberConfiguration configuration)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var asByteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(configuration));
            var hashBytes = md5.ComputeHash(asByteArray);
            var suffix = hashBytes.ToBase62();
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(configuration.EventType, configuration.SubscriberName, configuration.DLQMode.HasValue);

            return $"{readerServiceNameUri}-{suffix}";
        }

        public IList<string> FindOldNames(SubscriberConfiguration configuration, IEnumerable<string> serviceList)
        {
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(configuration.EventType, configuration.SubscriberName, configuration.DLQMode.HasValue);
            var pattern = $@"^{Regex.Escape(readerServiceNameUri)}(|-.+)$";
            return serviceList.Where(name => Regex.IsMatch(name, pattern)).ToList();
        }
    }
}