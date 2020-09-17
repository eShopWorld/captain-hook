using System;
using System.Collections.Generic;
using CaptainHook.Domain.Entities;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class SubscriberBuilder
    {
        private string _name = "captain-hook";
        private EventEntity _event = new EventEntity("event");
        private string _webhookSelectionRule = null;
        private string _callbackSelectionRule = null;
        private string _etag = null;
        private UriTransformEntity _webhooksUriTransformEntity = null;
        private UriTransformEntity _callbacksUriTransformEntity = null;
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

        public SubscriberBuilder WithEtag(string etag)
        {
            _etag = etag;
            return this;
        }

        public SubscriberBuilder WithWebhooksUriTransform(UriTransformEntity uriTransformEntity)
        {
            _webhooksUriTransformEntity = uriTransformEntity;
            return this;
        }

        public SubscriberBuilder WithCallbacksUriTransform(UriTransformEntity uriTransformEntity)
        {
            _callbacksUriTransformEntity = uriTransformEntity;
            return this;
        }

        public SubscriberBuilder WithWebhooksSelectionRule(string selectionRule)
        {
            _webhookSelectionRule = selectionRule;
            return this;
        }

        public SubscriberBuilder WithCallbacksSelectionRule(string selectionRule)
        {
            _callbackSelectionRule = selectionRule;
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
            var subscriber = new SubscriberEntity(_name, _event, _etag);
            var result = subscriber.SetHooks(new WebhooksEntity(WebhooksEntityType.Webhooks, _webhookSelectionRule, _webhooks, _webhooksUriTransformEntity));
            if(result.IsError)
            {
                throw new ArgumentException(result.Error.Message);
            }
            if (_callbacks?.Count > 0)
            {
                result = subscriber.SetHooks(new WebhooksEntity(WebhooksEntityType.Callbacks, _callbackSelectionRule, _callbacks, _callbacksUriTransformEntity));
                if (result.IsError)
                {
                    throw new ArgumentException(result.Error.Message);
                }
            }
            return subscriber;
        }
    }
}