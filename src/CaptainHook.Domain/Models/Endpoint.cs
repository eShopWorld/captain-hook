namespace CaptainHook.Domain.Models
{
    public class Endpoint
    {
        public string Uri { get; set; }
        public string Selector { get; set; }
        public string Authentication { get; set; }
        public string HttpVerb { get; set; }
    }
}
