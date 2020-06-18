using CaptainHook.Domain.Models;
using System.Collections.Generic;

namespace CaptainHook.Tests.Builders
{
    internal class SubscriberBuilder
    {
        private string _name = "captain-hook";
        private EventModel _event;
        private readonly List<EndpointModel> _webhooks = new List<EndpointModel>();
        private readonly List<EndpointModel> _callbacks = new List<EndpointModel>();
        private readonly List<EndpointModel> _dlq = new List<EndpointModel>();

        public SubscriberBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public SubscriberBuilder WithEvent(string name)
        {
            _event = new EventModel(name);
            return this;
        }

        public SubscriberBuilder WithWebhook(string uri, string httpVerb, string selector, AuthenticationModel authentication = null)
        {
            var endpoint = new EndpointModel(uri, authentication, httpVerb, selector);
            _webhooks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithCallback(string uri, string httpVerb, string selector, AuthenticationModel authentication = null)
        {
            var endpoint = new EndpointModel(uri, authentication, httpVerb, selector);
            _callbacks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithDlq(string uri, string httpVerb, string selector, AuthenticationModel authentication = null)
        {
            var endpoint = new EndpointModel(uri, authentication, httpVerb, selector);
            _dlq.Add(endpoint);
            return this;
        }

        public SubscriberModel Create()
        {
            var subscriber = new SubscriberModel(_name);
            subscriber.SetParentEvent(_event);

            _webhooks.ForEach(x => subscriber.AddWebhookEndpoint(x));
            _callbacks.ForEach(x => subscriber.AddCallbackEndpoint(x));
            _dlq.ForEach(x => subscriber.AddDlqEndpoint(x));

            return subscriber;
        }
    }
}