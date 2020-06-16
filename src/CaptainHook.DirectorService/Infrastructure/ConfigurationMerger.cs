using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Models;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class ConfigurationMerger
    {
        public IEnumerable<SubscriberConfiguration> Merge(IEnumerable<SubscriberConfiguration> kvModels, IEnumerable<Subscriber> cosmosModels)
        {
            var onlyInKv = kvModels.Where(k => !cosmosModels.Any(c => k.Name == c.Event.Name && k.SubscriberName == c.Name));
            var fromCosmos = cosmosModels.SelectMany(MapSubscriber);
            var result = onlyInKv.Union(fromCosmos);
            return result;
        }

        private IEnumerable<SubscriberConfiguration> MapSubscriber(Subscriber cosmosModel)
        {
            var result = new List<SubscriberConfiguration>();
            var config = MapBasicSubscriber(cosmosModel);
            result.Add(config);

            var dlq = cosmosModel?.Dlq?.Endpoints.FirstOrDefault();
            if (dlq != null)
            {
                result.Add(MapDlq(cosmosModel));
            }

            return result;
        }

        private SubscriberConfiguration MapBasicSubscriber(Subscriber cosmosModel)
        {
            var config = new SubscriberConfiguration
            {
                Name = cosmosModel.Event.Name,
                SubscriberName = cosmosModel.Name,
                Uri = cosmosModel.Webhooks.Endpoints.First().Uri,
                AuthenticationConfig = MapAuthentication(cosmosModel.Webhooks.Endpoints.FirstOrDefault()?.Authentication),
                Callback = MapCallback(cosmosModel),
            };
            return config;
        }

        private WebhookConfig MapCallback(Subscriber cosmosModel)
        {
            var endpoint = cosmosModel?.Callbacks?.Endpoints.FirstOrDefault();
            if (endpoint == null)
                return null;

            return new WebhookConfig()
            {
                Name = cosmosModel.Name,
                Uri = endpoint.Uri,
                HttpVerb = endpoint.HttpVerb,
                AuthenticationConfig = MapAuthentication(endpoint.Authentication),
                EventType = cosmosModel.Event.Type,
            };
        }

        private SubscriberConfiguration MapDlq(Subscriber cosmosModel)
        {
            var endpoint = cosmosModel?.Dlq?.Endpoints.FirstOrDefault();
            if (endpoint == null)
                return null;

            return new SubscriberConfiguration
            {
                Name = cosmosModel.Name,
                Uri = endpoint.Uri,
                HttpVerb = endpoint.HttpVerb,
                AuthenticationConfig = MapAuthentication(endpoint.Authentication),
                DLQMode = SubscriberDlqMode.WebHookMode,
            };
        }

        private AuthenticationConfig MapAuthentication(Authentication cosmosAuthentication)
        {
            if (cosmosAuthentication == null)
                return null;

            return new OidcAuthenticationConfig
            {
                Type = AuthenticationType.OIDC,
                ClientId = cosmosAuthentication.ClientId,
                ClientSecret = cosmosAuthentication.Secret,
                Uri = cosmosAuthentication.Uri,
                Scopes = cosmosAuthentication.Scopes,
            };
        }
    }
}
