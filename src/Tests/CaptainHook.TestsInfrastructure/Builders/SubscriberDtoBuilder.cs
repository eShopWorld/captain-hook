using CaptainHook.Contract;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class SubscriberDtoBuilder: SimpleBuilder<SubscriberDto>
    {
        public SubscriberDtoBuilder()
        {
            With(e => e.Webhooks, new WebhooksDtoBuilder().Create());
        }
    }
}
