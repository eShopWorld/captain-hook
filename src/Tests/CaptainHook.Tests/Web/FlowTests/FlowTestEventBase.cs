using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.Tests.Web.FlowTests
{
    public abstract class FlowTestEventBase : DomainEvent
    {
        [JsonProperty("payloadId")]
        public string PayloadId { get; set; }
        public string TenantCode { get; set;  } //for flow that utilize routing
    }
}