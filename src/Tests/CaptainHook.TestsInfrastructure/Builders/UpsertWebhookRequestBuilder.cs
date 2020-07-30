using CaptainHook.Application.Requests.Subscribers;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class UpsertWebhookRequestBuilder : SimpleBuilder<UpsertWebhookRequest>
    {
        public UpsertWebhookRequestBuilder()
        {
            With(x => x.EventName, "event");
            With(x => x.SubscriberName, "subscriber");
            With(x => x.Endpoint, new EndpointDtoBuilder().Create());
        }
    }
}