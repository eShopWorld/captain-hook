namespace CaptainHook.Domain.Models
{
    public class Endpoint
    {
        public Subscriber Subscriber { get; set; }
        public string Uri { get; set; }
        public Authentication Authentication { get; set; }
        public string HttpVerb { get; set; }
        public string Selector { get; set; }
    }
}
