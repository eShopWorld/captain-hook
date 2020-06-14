namespace CaptainHook.Domain.Models
{
    public class Subscriber
    {
        public string Name { get; set; }
        public Webhook Webhooks { get; set; }
        public Webhook Callbacks { get; set; }
        public Webhook Dlq { get; set; }
    }
}
