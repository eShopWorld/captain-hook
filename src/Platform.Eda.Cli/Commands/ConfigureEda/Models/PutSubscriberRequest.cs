using CaptainHook.Api.Client.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda.Models
{
    public class PutSubscriberRequest
    {
        public string EventName { get; set; }
        public string SubscriberName { get; set; }
        public CaptainHookContractSubscriberDto Subscriber { get; set; }
    }
}