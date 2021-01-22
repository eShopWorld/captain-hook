using System.Linq;
using System.Text.RegularExpressions;

namespace CaptainHook.Common.ServiceBus
{
    public class ServiceBusSettings
    {
        private static readonly Regex NamespaceRegex = new Regex(@"sb:\/\/(?<namespace>.*)\.servicebus", RegexOptions.Compiled);

        private string _connectionString;
        private string _serviceBusNamespace;

        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                _connectionString = value;
                _serviceBusNamespace = NamespaceRegex.Matches(_connectionString).FirstOrDefault()?.Groups["namespace"]?.Value;
            }
        }

        public string SubscriptionId { get; set; }

        public string ServiceBusNamespace => _serviceBusNamespace;
    }
}