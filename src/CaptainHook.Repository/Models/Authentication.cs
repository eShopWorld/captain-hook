namespace CaptainHook.Repository.Models
{
    public class Authentication
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Type { get; set; }
        public string Uri { get; set; }
        public string[] Scopes { get; set; }
    }
}
