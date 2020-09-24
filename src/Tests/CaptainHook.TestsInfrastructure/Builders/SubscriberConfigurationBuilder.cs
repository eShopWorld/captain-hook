using System;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class SubscriberConfigurationBuilder
    {
        private string _type = "event";
        private string _name = "event";
        private string _subscriberName = "captain-hook";
        private string _sourceSubscriptionName;
        private string _uri = "https://blah.blah.eshopworld.com";
        private string _httpVerb = "POST";
        private WebhookConfig _callback;
        private bool _asDlq;

        private readonly List<WebhookRequestRule> _webhookRequestRules = new List<WebhookRequestRule>();
        
        private AuthenticationConfig _authenticationConfig = new BasicAuthenticationConfig
        {
            Type = AuthenticationType.Basic,
            Username = "user",
            Password = "password",
        };

        public SubscriberConfigurationBuilder WithType(string type)
        {
            _type = type;
            return this;
        }

        public SubscriberConfigurationBuilder WithName(string name)
        {
            _name = name;
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

        public SubscriberConfigurationBuilder AsDLQ(bool asDlq = true)
        {
            _asDlq = asDlq;
            return this;
        }

        public SubscriberConfigurationBuilder WithoutAuthentication()
        {
            _authenticationConfig = new AuthenticationConfig { Type = AuthenticationType.None, };
            return this;
        }

        public SubscriberConfigurationBuilder WithOidcAuthentication()
        {
            return WithAuthentication(new OidcAuthenticationConfig
            {
                Type = AuthenticationType.OIDC,
                Uri = "https://blah-blah.sts.eshopworld.com",
                ClientId = "ClientId",
                ClientSecret = "ClientSecret",
                Scopes = new[] { "scope1", "scope2" }
            });
        }

        public SubscriberConfigurationBuilder WithBasicAuthentication()
        {
            return WithAuthentication(new BasicAuthenticationConfig
            {
                Type = AuthenticationType.Basic,
                Username = "username",
                Password = "password",
            });
        }

        public SubscriberConfigurationBuilder WithAuthentication(AuthenticationConfig authenticationConfig)
        {
            _authenticationConfig = authenticationConfig;
            return this;
        }

        public SubscriberConfigurationBuilder WithHttpVerb(string httpVerb)
        {
            _httpVerb = httpVerb;
            return this;
        }

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
            var builder = new WebhookRequestRuleBuilder();
            ruleBuilder(builder);
            _webhookRequestRules.Add(builder.Create());
            return this;
        }

        public SubscriberConfiguration CreateAsDlq()
        {
            _asDlq = true;
            _sourceSubscriptionName = _subscriberName;
            _subscriberName = "DLQ";
            _name = $"{_name}-{_subscriberName}";

            return Create();
        }

        public SubscriberConfiguration Create()
        {
            var subscriber = new SubscriberConfiguration
            {
                Name = _name,
                EventType = _type,
                SubscriberName = _subscriberName,
                SourceSubscriptionName = _sourceSubscriptionName,
                HttpVerb = _httpVerb,
                Uri = _uri,
                AuthenticationConfig = _authenticationConfig,
                Callback = _callback,
                WebhookRequestRules = _webhookRequestRules,
                DLQMode = _asDlq ? SubscriberDlqMode.WebHookMode : (SubscriberDlqMode?)null
            };

            return subscriber;
        }
    }
}