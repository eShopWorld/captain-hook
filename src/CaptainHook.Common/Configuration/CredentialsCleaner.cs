using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Authentication;

namespace CaptainHook.Common.Configuration
{
    public class CredentialsCleaner
    {
        private const string SecretDataReplacementString = "***";

        public static void HideCredentials(IEnumerable<SubscriberConfiguration> subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                HideWebhookCredentials(subscriber);
                HideWebhookCredentials(subscriber.Callback);

                var routes = subscriber.WebhookRequestRules?.SelectMany(r => r.Routes) ?? Enumerable.Empty<WebhookConfigRoute>();
                var callbackRoutes = subscriber.Callback?.WebhookRequestRules?.SelectMany(r => r.Routes) ?? Enumerable.Empty<WebhookConfigRoute>();
                var allRoutes = routes.Union(callbackRoutes);

                foreach (var route in allRoutes)
                {
                    HideWebhookCredentials(route);
                }
            }
        }

        private static void HideWebhookCredentials(WebhookConfig configuration)
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