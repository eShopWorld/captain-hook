namespace CaptainHook.Common
{
    public class WebHookConfig
    {
        public string DomainEvent { get; set; }

        public string Uri { get; set; }

        public bool RequiresAuth { get; set; } = true;

        public AuthConfig AuthConfig { get; set; }

        public string Name { get; set; }
    }
}