namespace CaptainHook.Domain.Models
{
    /// <summary>
    /// Endpoint model
    /// </summary>
    public class EndpointModel
    {
        /// <summary>
        /// Parent subscriber model
        /// </summary>
        public SubscriberModel ParentSubscriber { get; private set; }

        /// <summary>
        /// Endpoint URI
        /// </summary>
        public string Uri { get; }

        /// <summary>
        /// Endpoint authentication
        /// </summary>
        public AuthenticationModel Authentication { get; }

        /// <summary>
        /// Endpoint HTTP method
        /// </summary>
        public string HttpVerb { get; }

        /// <summary>
        /// Endpoint selector
        /// </summary>
        public string Selector { get; }

        public EndpointModel(string uri, AuthenticationModel authentication, string httpVerb, string selector): this(uri, authentication, httpVerb, selector, null) { }

        public EndpointModel(string uri, AuthenticationModel authentication, string httpVerb, string selector, SubscriberModel subscriber)
        {
            Uri = uri;
            Authentication = authentication;
            HttpVerb = httpVerb;
            Selector = selector;
            
            SetParentSubscriber(subscriber);
        }

        public void SetParentSubscriber(SubscriberModel subscriber)
        {
            ParentSubscriber = subscriber;
        }
    }
}
