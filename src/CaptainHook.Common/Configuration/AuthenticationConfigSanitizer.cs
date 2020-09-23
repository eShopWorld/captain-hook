using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Authentication;

namespace CaptainHook.Common.Configuration
{
    public static class AuthenticationConfigSanitizer
    {
        private static readonly string SecretDataReplacementString = "***";

        public static void Sanitize(IEnumerable<SubscriberConfiguration> subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                Sanitize(subscriber);
            }
        }

        public static void Sanitize(SubscriberConfiguration subscriber)
        {
            SanitizeWebhookCredentials(subscriber);
            SanitizeWebhookCredentials(subscriber.Callback);

            var routes = subscriber.WebhookRequestRules?.SelectMany(r => r.Routes) ?? Enumerable.Empty<WebhookConfigRoute>();
            var callbackRoutes = subscriber.Callback?.WebhookRequestRules?.SelectMany(r => r.Routes) ?? Enumerable.Empty<WebhookConfigRoute>();
            var allRoutes = routes.Union(callbackRoutes);

            foreach (var route in allRoutes)
            {
                SanitizeWebhookCredentials(route);
            }
        }

        private static void SanitizeWebhookCredentials(WebhookConfig configuration)
        {
            if (configuration == null)
                return;

            switch (configuration.AuthenticationConfig)
            {
                case OidcAuthenticationConfig oidcAuth:
                    oidcAuth.ClientSecret = SecretDataReplacementString;
                    break;
                case BasicAuthenticationConfig basicAuth:
                    basicAuth.Password = SecretDataReplacementString;
                    break;
            }
        }
    }
}