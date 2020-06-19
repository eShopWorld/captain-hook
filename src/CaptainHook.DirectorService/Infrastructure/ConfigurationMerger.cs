using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class ConfigurationMerger
    {
        /// <summary>
        /// Merges subscribers loaded from Cosmos and from KeyVault. If particular subscriber is defined in both sources, the Cosmos version overrides the KeyVault version.
        /// </summary>
        /// <param name="subscribersFromKeyVault">Subscriber definitions loaded from KeyVault</param>
        /// <param name="subscribersFromCosmos">Subscriber models retrieved from Cosmos</param>
        /// <returns>List of all subscribers converted to KeyVault structure</returns>
        public ReadOnlyCollection<SubscriberConfiguration> Merge(IEnumerable<SubscriberConfiguration> subscribersFromKeyVault, IEnumerable<SubscriberEntity> subscribersFromCosmos)
        {
            var onlyInKv = subscribersFromKeyVault
                .Where(kvSubscriber => !subscribersFromCosmos.Any(cosmosSubscriber =>
                    kvSubscriber.Name.Equals(cosmosSubscriber.ParentEvent.Name, StringComparison.InvariantCultureIgnoreCase)
                    && kvSubscriber.SubscriberName.Equals(cosmosSubscriber.Name, StringComparison.InvariantCultureIgnoreCase)));

            var fromCosmos = subscribersFromCosmos.SelectMany(MapSubscriber);
            var union = onlyInKv.Union(fromCosmos).ToList();
            return new ReadOnlyCollection<SubscriberConfiguration>(union);
        }

        private IEnumerable<SubscriberConfiguration> MapSubscriber(SubscriberEntity cosmosModel)
        {
            yield return MapBasicSubscriberData(cosmosModel);

            // DLQ handling not needed now
            //var dlq = cosmosModel?.Dlq?.Endpoints.FirstOrDefault();
            //if (dlq != null)
            //{
            //    yield return MapDlq(cosmosModel);
            //}
        }

        private SubscriberConfiguration MapBasicSubscriberData(SubscriberEntity cosmosModel)
        {
            var config = new SubscriberConfiguration
            {
                Name = cosmosModel.ParentEvent.Name,
                SubscriberName = cosmosModel.Name,
                Uri = cosmosModel.Webhooks.Endpoints.First().Uri,
                AuthenticationConfig = MapAuthentication(cosmosModel.Webhooks.Endpoints.FirstOrDefault()?.Authentication),
                // Callback handling not needed now
                //Callback = MapCallback(cosmosModel),
            };
            return config;
        }

        //private WebhookConfig MapCallback(SubscriberModel cosmosModel)
        //{
        //    var endpoint = cosmosModel?.Callbacks?.Endpoints.FirstOrDefault();
        //    if (endpoint == null)
        //        return null;

        //    return new WebhookConfig()
        //    {
        //        Name = cosmosModel.Name,
        //        Uri = endpoint.Uri,
        //        HttpVerb = endpoint.HttpVerb,
        //        AuthenticationConfig = MapAuthentication(endpoint.Authentication),
        //        EventType = cosmosModel.ParentEvent.Name,
        //    };
        //}

        //private SubscriberConfiguration MapDlq(SubscriberModel cosmosModel)
        //{
        //    var endpoint = cosmosModel?.Dlq?.Endpoints.FirstOrDefault();
        //    if (endpoint == null)
        //        return null;

        //    return new SubscriberConfiguration
        //    {
        //        Name = cosmosModel.Name,
        //        Uri = endpoint.Uri,
        //        HttpVerb = endpoint.HttpVerb,
        //        AuthenticationConfig = MapAuthentication(endpoint.Authentication),
        //        DLQMode = SubscriberDlqMode.WebHookMode,
        //    };
        //}

        private AuthenticationConfig MapAuthentication(AuthenticationEntity cosmosAuthentication)
        {
            if (cosmosAuthentication == null)
                return null;

            return new OidcAuthenticationConfig
            {
                Type = AuthenticationType.OIDC,
                ClientId = cosmosAuthentication.ClientId,
                ClientSecret = cosmosAuthentication.SecretStore.SecretName,
                Uri = cosmosAuthentication.Uri,
                Scopes = cosmosAuthentication.Scopes,
            };
        }
    }
}
