using CaptainHook.Api.Client.Models;

namespace CaptainHook.Cli.Commands.ExecuteApi.Models
{
    public class PutSubscriberRequest
    {
        public string EventName { get; set; }
        public string SubscriberName { get; set; }
        public CaptainHookContractSubscriberDto Subscriber { get; set; }
    }
}