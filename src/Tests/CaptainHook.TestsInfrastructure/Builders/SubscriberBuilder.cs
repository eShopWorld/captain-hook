using System.Collections.Generic;
using CaptainHook.Domain.Entities;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class SubscriberBuilder
    {
        private string _name = "captain-hook";
        private EventEntity _event = new EventEntity("event");
        private readonly List<EndpointEntity> _webhooks = new List<EndpointEntity>();
        private readonly List<EndpointEntity> _callbacks = new List<EndpointEntity>();
        private readonly List<EndpointEntity> _dlq = new List<EndpointEntity>();

        public SubscriberBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public SubscriberBuilder WithEvent(string name)
        {
            _event = new EventEntity(name);
            return this;
        }

        public SubscriberBuilder WithWebhook(string uri, string httpVerb, string selector, UriTransformEntity uriTransform = null, AuthenticationEntity authentication = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector, null, uriTransform);
            _webhooks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithCallback(string uri, string httpVerb, string selector, UriTransformEntity uriTransform = null, AuthenticationEntity authentication = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector, null, uriTransform);
            _callbacks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithDlq(string uri, string httpVerb, string selector, UriTransformEntity uriTransform = null, AuthenticationEntity authentication = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector, null, uriTransform);
            _dlq.Add(endpoint);
            return this;
        }

        public SubscriberEntity Create()
        {
            var subscriber = new SubscriberEntity(_name);
            subscriber.SetParentEvent(_event);

            _webhooks.ForEach(x => subscriber.AddWebhookEndpoint(x));

            return subscriber;
        }
    }
}