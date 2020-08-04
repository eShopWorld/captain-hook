namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Endpoint model
    /// </summary>
    public class EndpointEntity
    {
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

        public UriTransformEntity UriTransform { get; }

        public EndpointEntity(string uri, AuthenticationEntity authentication, string httpVerb, string selector, 
            SubscriberEntity subscriber = null, UriTransformEntity uriTransform = null)
        {
            Uri = uri;
            Authentication = authentication;
            HttpVerb = httpVerb;
            Selector = selector;
            UriTransform = uriTransform;

            SetParentSubscriber(subscriber);
        }

        public void SetParentSubscriber(SubscriberEntity subscriber)
        {
            ParentSubscriber = subscriber;
        }
    }
}
