using System.Collections.Generic;
using CaptainHook.Domain.Entities;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class SubscriberBuilder
    {
        private string _name = "captain-hook";
        private EventEntity _event = new EventEntity("event");
        private string _webhookSelectionRule = null;
        private UriTransformEntity _uriTransformEntity;
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

        public SubscriberBuilder WithWebhookUriTransform(UriTransformEntity uriTransformEntity)
        {
            _uriTransformEntity = uriTransformEntity;
            return this;
        }

        public SubscriberBuilder WithWebhookSelectionRule(string selectionRule)
        {
            _webhookSelectionRule = selectionRule;
            return this;
        }

        public SubscriberBuilder WithWebhook(string uri, string httpVerb, string selector, AuthenticationEntity authentication = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector, null);
            _webhooks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithCallback(string uri, string httpVerb, string selector, AuthenticationEntity authentication = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector, null);
            _callbacks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithDlq(string uri, string httpVerb, string selector, AuthenticationEntity authentication = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector, null);
            _dlq.Add(endpoint);
            return this;
        }

        public SubscriberEntity Create()
        {
            var subscriber = new SubscriberEntity(_name);
            subscriber.SetParentEvent(_event);
            subscriber.AddWebhooks(new WebhooksEntity(_webhookSelectionRule, _webhooks, _uriTransformEntity));
            return subscriber;
        }
    }
}