// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace CaptainHook.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class CaptainHookContractSubscriberDto
    {
        /// <summary>
        /// Initializes a new instance of the CaptainHookContractSubscriberDto
        /// class.
        /// </summary>
        public CaptainHookContractSubscriberDto()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the CaptainHookContractSubscriberDto
        /// class.
        /// </summary>
        public CaptainHookContractSubscriberDto(CaptainHookContractWebhooksDto webhooks = default(CaptainHookContractWebhooksDto))
        {
            Webhooks = webhooks;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "webhooks")]
        public CaptainHookContractWebhooksDto Webhooks { get; set; }

    }
}
