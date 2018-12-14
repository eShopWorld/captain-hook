namespace CaptainHook.Common
{
    using System.Collections.Generic;

    public class WebHookConfig
    {
        public string Uri { get; set; }

        public bool RequiresAuth { get; set; } = true;

        public AuthConfig AuthConfig { get; set; }

        public string Name { get; set; }

        public WebHookConfig Callback { get; set; }

        public DomainEventConfig  DomainEventConfig { get; set; }
    }

    public class DomainEventConfig
    {
        /// <summary>
        /// name of the domain event
        /// </summary>
        public string[] DomainEvent { get; set; }

        /// <summary>
        /// DomainEventPath within the payload to query to get data for delivery
        /// </summary>
        public string[] Path { get; set; }

        /// <summary>
        /// todo clean this up.
        /// </summary>
        /// <param name="domainEventName"></param>
        /// <returns></returns>
        public string GetPath(string domainEventName)
        {
            var index = 0;

            foreach (var s in DomainEvent)
            {
                if (s.Equals(domainEventName))
                {
                    return Path[index];
                }

                index++;
            }

            return null;
        }
    }
}