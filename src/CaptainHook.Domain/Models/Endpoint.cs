namespace CaptainHook.Domain.Models
{
    public class Endpoint
    {
        public Webhooks Webhooks { get; set; }
        public string Uri { get; set; }
        public Authentication Authentication { get; set; }
        public string HttpVerb { get; set; }
    }
}
