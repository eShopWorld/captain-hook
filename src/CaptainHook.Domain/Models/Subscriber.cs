namespace CaptainHook.Domain.Models
{
    public class Subscriber
    {
        public string Name { get; set; }
        public Event Event { get; set; }
        public Webhooks Webhooks { get; set; }
        public Webhooks Callbacks { get; set; }
        public Webhooks Dlq { get; set; }
    }
}
