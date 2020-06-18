using CaptainHook.Domain.Models;

namespace CaptainHook.Tests.Builders
{
    internal class SubscriberBuilder
    {
        private string _name = "captain-hook";
        private Event _event;
        private Webhooks _webhooks;
        private Webhooks _callbacks;
        private Webhooks _dlq;

        public SubscriberBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public SubscriberBuilder WithEvent(string name)
        {
            _event = new Event { Name = name, Type = name };
            return this;
        }

        public SubscriberBuilder WithWebhook(string uri, string httpVerb, Authentication authentication = null)
        {
            var endpoint = new Endpoint { Uri = uri, HttpVerb = httpVerb, Authentication = authentication };
            _webhooks = new Webhooks();
            _webhooks.Endpoints.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithCallback(string uri, string httpVerb, Authentication authentication = null)
        {
            var endpoint = new Endpoint { Uri = uri, HttpVerb = httpVerb, Authentication = authentication };
            _callbacks = new Webhooks();
            _callbacks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithDlq(string uri, string httpVerb, Authentication authentication = null)
        {
            var endpoint = new Endpoint { Uri = uri, HttpVerb = httpVerb, Authentication = authentication };
            _dlq = new Webhooks();
            _dlq.Endpoints.Add(endpoint);
            return this;
        }

        public Subscriber Create()
        {
            var subscriber = new Subscriber
            {
                Name = _name,
                Event = _event,
                Webhooks = _webhooks,
                Callbacks = _callbacks,
                Dlq = _dlq,
            };

            return subscriber;
        }
    }
}