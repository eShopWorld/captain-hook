using Newtonsoft.Json;

namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Endpoint model in Cosmos DB
    /// </summary>
    internal class EndpointSubdocument
    {
        /// <summary>
        /// Endpoint URI
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Endpoint selector
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Selector { get; set; }

        /// <summary>
        /// Endpoint authentication
        /// </summary>
        public AuthenticationSubdocument Authentication { get; set; }

        /// <summary>
        /// Endpoint HTTP verb
        /// </summary>
        public string HttpVerb { get; set; }
    }
}
