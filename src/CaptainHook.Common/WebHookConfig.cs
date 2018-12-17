namespace CaptainHook.Common
{
    using System.Collections.Generic;

    public class WebHookConfig
    {
        public WebHookConfig()
        {
            this.DomainEvents = new List<DomainEventConfig>();
        }

        public string Uri { get; set; }

        public bool RequiresAuth { get; set; } = true;

        public AuthConfig Auth { get; set; }

        public WebHookConfig Callback { get; set; }
        
        public List<DomainEventConfig> DomainEvents { get; set; }

        public string Name { get; set; }
    }

    public class DomainEventConfig
    {
        /// <summary>
        /// name of the domain event
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DomainEventPath within the payload to query to get data for delivery
        /// </summary>
        public string Path { get; set; }
    }
}