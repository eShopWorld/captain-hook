namespace CaptainHook.Contract
{
    public class EndpointDto
    {
        public string Uri { get; set; }

        public string HttpVerb { get; set; }

        public string Selector { get; set; }

        public AuthenticationDto Authentication{ get; set; }
    }
}
