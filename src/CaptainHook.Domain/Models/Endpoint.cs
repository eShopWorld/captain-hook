namespace CaptainHook.Domain.Models
{
    public class Endpoint
    {
        public string Uri { get; set; }
        public string Selector { get; set; }
        public Authentication Authentication { get; set; }
        public string HttpVerb { get; set; }
    }
}
