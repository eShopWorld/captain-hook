using CaptainHook.Storage.Cosmos.Models;

namespace CaptainHook.TestsInfrastructure.Builders
{
    internal class WebhooksSubdocumentBuilder : SimpleBuilder<WebhookSubdocument>
    {
        public WebhooksSubdocumentBuilder()
        {
            With(e => e.SelectionRule, "$.TenantCode");
            With(e => e.Endpoints, new EndpointSubdocument[] { new EndpointSubdocumentBuilder().Create() });
        }
    }
}
