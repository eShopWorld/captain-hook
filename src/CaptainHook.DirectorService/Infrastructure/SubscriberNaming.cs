namespace CaptainHook.DirectorService.Infrastructure
{
    public class SubscriberNaming
    { 
        public string SubscriberName { get; set; }
        public string EventType { get; set; }
        public bool IsDlqMode { get; set; }
    }
}
