﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class SubscriberConfigurationBuilder
    {
        private string _type = "event1";
        private string _subscriberName = "captain-hook";
        private string _uri = "https://blah.blah.eshopworld.com";
        private HttpMethod _httpMethod = HttpMethod.Post;
        private AuthenticationConfig _authenticationConfig = new BasicAuthenticationConfig
        {
            Type = AuthenticationType.Basic,
            Username = "user",
            Password = "password",
        };
        private List<WebhookRequestRule> _webhookRequestRules;
        private WebhookConfig _callback;
        private bool _asDlq;

        public SubscriberConfigurationBuilder WithType(string type)
        {
            _type = type;
            return this;
        }

        public SubscriberConfigurationBuilder WithSubscriberName(string subscriberName)
        {
            _subscriberName = subscriberName;
            return this;
        }

        public SubscriberConfigurationBuilder WithUri(string uri)
        {
            _uri = uri;
            return this;
        }

        public SubscriberConfigurationBuilder AsDLQ (bool asDlq = true)
        {
            _asDlq = asDlq;
            return this;
        }

        public SubscriberConfigurationBuilder WithOidcAuthentication()
        {
            _authenticationConfig = new OidcAuthenticationConfig
            {
                Type = AuthenticationType.OIDC,
                Uri = "https://blah-blah.sts.eshopworld.com",
                ClientId = "ClientId",
                ClientSecret = "ClientSecret",
                Scopes = new[] { "scope1", "scope2" }
            };

            return this;
        }

        public SubscriberConfigurationBuilder WithBasicAuthentication()
        {
            _authenticationConfig = new BasicAuthenticationConfig
            {
                Type = AuthenticationType.Basic,
                Username = "username",
                Password = "password",
            };

            return this;
        }

        // TODO: remove or use version of this method which accepts SubscriberConfigurationBuilder
        public SubscriberConfigurationBuilder WithCallback(string uri = "https://callback.eshopworld.com")
        {
            _callback = new WebhookConfig
            {
                Name = "callback",
                HttpMethod = HttpMethod.Post,
                Uri = uri,
                EventType = "event1",
                AuthenticationConfig = new AuthenticationConfig
                {
                    Type = AuthenticationType.None
                },
            };

            return this;
        }

        public SubscriberConfigurationBuilder WithCallback(Action<SubscriberConfigurationBuilder> callbackBuilder)
        {
            var builder = new SubscriberConfigurationBuilder();
            callbackBuilder(builder);
            _callback = builder.Create();
            return this;
        }

        public SubscriberConfigurationBuilder AddWebhookRequestRule(Action<WebhookRequestRuleBuilder> ruleBuilder)
        {
            if (_webhookRequestRules == null)
            {
                _webhookRequestRules = new List<WebhookRequestRule>();
            }

            var builder = new WebhookRequestRuleBuilder();
            ruleBuilder(builder);
            _webhookRequestRules.Add(builder.Create());
            return this;
        }

        public SubscriberConfiguration Create()
        {
            var subscriber = new SubscriberConfiguration
            {
                Name = _type,
                EventType = _type,
                SubscriberName = _subscriberName,
                HttpMethod = _httpMethod,
                Uri = _uri,
                AuthenticationConfig = _authenticationConfig,
                Callback = _callback,
                WebhookRequestRules = _webhookRequestRules,
                DLQMode = _asDlq ? SubscriberDlqMode.WebHookMode: (SubscriberDlqMode?) null
            };

            return subscriber;
        }

    }
}