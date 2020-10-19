using System;

namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Endpoint model
    /// </summary>
    public class EndpointEntity
    {
        private const string DefaultEndpointSelector = "*";

        public static bool IsDefaultSelector(string selector)
        {
            return selector == null || selector.Equals(DefaultEndpointSelector, StringComparison.OrdinalIgnoreCase);
        }

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

        /// <summary>
        /// Retry sleep durations to use for request retry logic
        /// </summary>
        public TimeSpan[] RetrySleepDurations { get; }

        /// <summary>
        /// Timeout for HTTP calls
        /// </summary>
        public TimeSpan? Timeout { get; }

        public EndpointEntity(string uri, AuthenticationEntity authentication, string httpVerb, string selector, TimeSpan[] retrySleepDurations = null, TimeSpan? timeout = null, SubscriberEntity subscriber = null)
        {
            Uri = uri;
            Authentication = authentication;
            HttpVerb = httpVerb;
            Selector = selector ?? DefaultEndpointSelector;
            RetrySleepDurations = retrySleepDurations;
            Timeout = timeout;

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
