namespace CaptainHook.Domain.Models
{
    public class Subscriber
    {
        public string Name { get; set; }
        public Webhook Webhooks { get; set; } = new Webhook();
        public Webhook Callbacks { get; set; } = new Webhook();
        public Webhook Dlq { get; set; } = new Webhook();
    }
}
