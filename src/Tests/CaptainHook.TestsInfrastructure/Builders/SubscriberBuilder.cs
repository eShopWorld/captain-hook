﻿using System;
using System.Collections.Generic;
using CaptainHook.Domain.Entities;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class SubscriberBuilder
    {
        private string _name = "captain-hook";
        private EventEntity _event = new EventEntity("event");
        private string _etag;
        private string _webhookSelectionRule;
        private string _callbackSelectionRule;
        private string _dlqhooksSelectionRule;
        private string _webhookPayloadTransform = "$";
        private string _dlqhookPayloadTransform = "$";
        private int? _webhookMaxDeliveryCount = null;
        private UriTransformEntity _webhooksUriTransformEntity;
        private UriTransformEntity _callbacksUriTransformEntity;
        private UriTransformEntity _dlqhooksTransformEntity;
        private readonly List<EndpointEntity> _webhooks = new List<EndpointEntity>();
        private readonly List<EndpointEntity> _callbacks = new List<EndpointEntity>();
        private readonly List<EndpointEntity> _dlqhooks = new List<EndpointEntity>();

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

        public SubscriberBuilder WithMaxDeliveryCount(int maxDeliveryCount)
        {
            _webhookMaxDeliveryCount = maxDeliveryCount;
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

        public SubscriberBuilder WithDlqhooksUriTransform(UriTransformEntity uriTransformEntity)
        {
            _dlqhooksTransformEntity = uriTransformEntity;
            return this;
        }

        public SubscriberBuilder WithWebhooksPayloadTransform(string payloadTransform)
        {
            _webhookPayloadTransform = payloadTransform;
            return this;
        }

        public SubscriberBuilder WithDlqhooksPayloadTransform(string payloadTransform)
        {
            _dlqhookPayloadTransform = payloadTransform;
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

        public SubscriberBuilder WithDlqhooksSelectionRule(string selectionRule)
        {
            _dlqhooksSelectionRule = selectionRule;
            return this;
        }

        public SubscriberBuilder WithWebhook(string uri, string httpVerb, string selector, AuthenticationEntity authentication = null, TimeSpan[] retrySleepDurations = null, TimeSpan? timeout = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector, retrySleepDurations, timeout);
            _webhooks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithCallback(string uri, string httpVerb, string selector, AuthenticationEntity authentication = null, TimeSpan[] retrySleepDurations = null, TimeSpan? timeout = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector, retrySleepDurations, timeout);
            _callbacks.Add(endpoint);
            return this;
        }

        public SubscriberBuilder WithDlqhook(string uri, string httpVerb, string selector, AuthenticationEntity authentication = null, TimeSpan[] retrySleepDurations = null, TimeSpan? timeout = null)
        {
            var endpoint = new EndpointEntity(uri, authentication, httpVerb, selector, retrySleepDurations, timeout);
            _dlqhooks.Add(endpoint);
            return this;
        }

        public SubscriberEntity Create()
        {
            var subscriber = new SubscriberEntity(_name, _event, _etag)
            {
                MaxDeliveryCount = _webhookMaxDeliveryCount
            };

            return subscriber.SetHooks(
                    new WebhooksEntity(WebhooksEntityType.Webhooks, _webhookSelectionRule, _webhooks, _webhooksUriTransformEntity, _webhookPayloadTransform))
                .Then(_ => subscriber.SetHooks(
                    new WebhooksEntity(WebhooksEntityType.Callbacks, _callbackSelectionRule, _callbacks, _callbacksUriTransformEntity, null)))
                .Then(_ => subscriber.SetHooks(
                    new WebhooksEntity(WebhooksEntityType.DlqHooks, _dlqhooksSelectionRule, _dlqhooks, _dlqhooksTransformEntity, _dlqhookPayloadTransform)))
                .Match(
                    error => throw new ArgumentException(error.Message),
                    s => s
                );
        }
    }
}