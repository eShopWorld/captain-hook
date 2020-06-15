namespace CaptainHook.Domain.Models
{
    public class Subscriber
    {
        public string Name { get; set; }
        public Event Event { get; set; }
        public Webhooks Webhooks { get; set; } = new Webhooks();
        public Webhooks Callbacks { get; set; } = new Webhooks();
        public Webhooks Dlq { get; set; } = new Webhooks();
    }
}
