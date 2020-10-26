// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace CaptainHook.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class CaptainHookContractEndpointDto
    {
        /// <summary>
        /// Initializes a new instance of the CaptainHookContractEndpointDto
        /// class.
        /// </summary>
        public CaptainHookContractEndpointDto()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the CaptainHookContractEndpointDto
        /// class.
        /// </summary>
        public CaptainHookContractEndpointDto(string uri = default(string), string httpVerb = default(string), object authentication = default(object), string selector = default(string), string timeout = default(string), IList<string> retrySleepDurations = default(IList<string>))
        {
            Uri = uri;
            HttpVerb = httpVerb;
            Authentication = authentication;
            Selector = selector;
            Timeout = timeout;
            RetrySleepDurations = retrySleepDurations;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "httpVerb")]
        public string HttpVerb { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "authentication")]
        public object Authentication { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "selector")]
        public string Selector { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "timeout")]
        public string Timeout { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "retrySleepDurations")]
        public IList<string> RetrySleepDurations { get; set; }

    }
}
