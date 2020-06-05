using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure;

namespace CaptainHook.DirectorService.Extensions
{
    public static class SubscriberConfigurationExtensions
    {
        public static SubscriberNaming ToSubscriberNaming(this SubscriberConfiguration subscriberConfiguration)
        {
            return new SubscriberNaming
            {
                SubscriberName = subscriberConfiguration.SubscriberName,
                EventType = subscriberConfiguration.EventType,
                IsDlqMode = subscriberConfiguration.DLQMode.HasValue
            };
        }
    }
}
