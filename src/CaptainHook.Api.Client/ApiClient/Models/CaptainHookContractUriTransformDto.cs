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

    public partial class CaptainHookContractUriTransformDto
    {
        /// <summary>
        /// Initializes a new instance of the
        /// CaptainHookContractUriTransformDto class.
        /// </summary>
        public CaptainHookContractUriTransformDto()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// CaptainHookContractUriTransformDto class.
        /// </summary>
        public CaptainHookContractUriTransformDto(IDictionary<string, string> replace = default(IDictionary<string, string>))
        {
            Replace = replace;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "replace")]
        public IDictionary<string, string> Replace { get; set; }

    }
}