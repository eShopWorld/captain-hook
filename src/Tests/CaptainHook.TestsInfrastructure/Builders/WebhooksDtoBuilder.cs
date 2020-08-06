using CaptainHook.Contract;
using System.Collections.Generic;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class WebhooksDtoBuilder : SimpleBuilder<WebhooksDto>
    {
        public WebhooksDtoBuilder()
        {
            With(e => e.SelectionRule, "$.TenantCode");
            With(e => e.Endpoints, new List<EndpointDto>() { new EndpointDtoBuilder().Create() });
        }
    }
}
