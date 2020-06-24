using CaptainHook.Domain.Entities;
using System.Collections.Generic;

namespace CaptainHook.Tests.Builders
{
    internal class SubscriberBuilder
    {
        private string _subscriberId;
        private string _name = "captain-hook";
        private EventEntity _event;
        private readonly List<EndpointEntity> _webhooks = new List<EndpointEntity>();
        private readonly List<EndpointEntity> _callbacks = new List<EndpointEntity>();
        private readonly List<EndpointEntity> _dlq = new List<EndpointEntity>();

        public SubscriberBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public SubscriberBuilder WithSubscriberId(string subscriberId)
        {
            _subscriberId = subscriberId;
            return this;
        }

        public SubscriberBuilder WithEvent(string name)
        {
            _event = new EventEntity(name);
            return this;
        }

        public SubscriberBuilder WithWebhook(string uri, string httpVerb, string selector, AuthenticationEntity authentication = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector);
            _webhooks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithCallback(string uri, string httpVerb, string selector, AuthenticationEntity authentication = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector);
            _callbacks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithDlq(string uri, string httpVerb, string selector, AuthenticationEntity authentication = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector);
            _dlq.Add(endpoint);
            return this;
        }

        public SubscriberEntity Create()
        {
            var subscriber = new SubscriberEntity(_subscriberId, _name);
            subscriber.SetParentEvent(_event);

            _webhooks.ForEach(x => subscriber.AddWebhookEndpoint(x));

            return subscriber;
        }
    }
}