using CaptainHook.Storage.Cosmos.Models;

namespace CaptainHook.TestsInfrastructure.Builders
{
    internal class SubscriberDocumentBuilder : SimpleBuilder<SubscriberDocument>
    {
        public SubscriberDocumentBuilder()
        {
            var eventName = "a-test-event-name";
            var subscriberName = "a-test-subscriber-name";

            With(x => x.EventName, eventName);
            With(x => x.SubscriberName, subscriberName);
            With(x => x.Id, $"{eventName}-{subscriberName}");
            With(x => x.Webhooks, new WebhooksSubdocumentBuilder().Create());
            With(x => x.Callbacks, new WebhooksSubdocumentBuilder().Create());
        }
    }
}
