using CaptainHook.Api.Client.Models;

namespace CaptainHook.Cli.Commands.ExecuteApi
{
    public class PutSubscriberRequest
    {
        public string Event { get; set; }
        public string Subscriber { get; set; }
        public CaptainHookContractSubscriberDto SubscriberDto { get; set; }
    }
}