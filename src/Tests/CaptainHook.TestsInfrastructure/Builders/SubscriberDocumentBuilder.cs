using CaptainHook.Storage.Cosmos.Models;

namespace CaptainHook.TestsInfrastructure.Builders
{
    internal class SubscriberDocumentBuilder : SimpleBuilder<SubscriberDocument>
    {
        public SubscriberDocumentBuilder()
        {
            With(e => e.Webhooks, new WebhooksSubdocumentBuilder().Create());
            With(e => e.Callbacks, new WebhooksSubdocumentBuilder().Create());
        }
    }
}
