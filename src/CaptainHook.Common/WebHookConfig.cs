namespace CaptainHook.Common
{
    public class WebHookConfig
    {
        public string DomainEvent { get; set; }

        public string Uri { get; set; }

        public bool RequiresAuth { get; set; } = true;

        public AuthConfig AuthConfig { get; set; }

        public string Name { get; set; }

        public WebHookConfig Callback { get; set; }

        /// <summary>
        /// DomainEventPath within the payload to query to get data for delivery
        /// </summary>
        public string DomainEventPath { get; set; }
    }
}