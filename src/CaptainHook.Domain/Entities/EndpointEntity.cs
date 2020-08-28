namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Endpoint model
    /// </summary>
    public class EndpointEntity
    {
        public const string DefaultEndpointSelector = "*";

        /// <summary>
        /// Parent subscriber model
        /// </summary>
        public SubscriberEntity ParentSubscriber { get; private set; }

        /// <summary>
        /// Endpoint URI
        /// </summary>
        public string Uri { get; }

        /// <summary>
        /// Endpoint authentication
        /// </summary>
        public AuthenticationEntity Authentication { get; }

        /// <summary>
        /// Endpoint HTTP method
        /// </summary>
        public string HttpVerb { get; }

        /// <summary>
        /// Endpoint selector
        /// </summary>
        public string Selector { get; }

        public EndpointEntity(string uri, AuthenticationEntity authentication, string httpVerb, string selector, SubscriberEntity subscriber = null)
        {
            Uri = uri;
            Authentication = authentication;
            HttpVerb = httpVerb;
            Selector = selector;

            SetParentSubscriber(subscriber);
        }

        public static EndpointEntity FromSelector(string selector) => new EndpointEntity(null, null, null, selector);

        public EndpointEntity SetParentSubscriber(SubscriberEntity subscriber)
        {
            ParentSubscriber = subscriber;
            return this;
        }
    }
}
