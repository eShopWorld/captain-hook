using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Domain.Entities;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class ConfigurationMerger : IConfigurationMerger
    {
        private readonly ISecretManager _secretManager;

        public ConfigurationMerger(ISecretManager secretManager)
        {
            _secretManager = secretManager;
        }

        /// <summary>
        /// Merges subscribers loaded from Cosmos and from KeyVault. If particular subscriber is defined in both sources, the Cosmos version overrides the KeyVault version.
        /// </summary>
        /// <param name="subscribersFromKeyVault">Subscriber definitions loaded from KeyVault</param>
        /// <param name="subscribersFromCosmos">Subscriber models retrieved from Cosmos</param>
        /// <returns>List of all subscribers converted to KeyVault structure</returns>
        public async Task<ReadOnlyCollection<SubscriberConfiguration>> MergeAsync(
            IEnumerable<SubscriberConfiguration> subscribersFromKeyVault,
            IEnumerable<SubscriberEntity> subscribersFromCosmos)
        {
            var onlyInKv = subscribersFromKeyVault
                .Where(kvSubscriber => !subscribersFromCosmos.Any(cosmosSubscriber =>
                    kvSubscriber.Name.Equals(cosmosSubscriber.ParentEvent.Name, StringComparison.InvariantCultureIgnoreCase)
                    && kvSubscriber.SubscriberName.Equals(cosmosSubscriber.Name, StringComparison.InvariantCultureIgnoreCase)));

            async Task<IEnumerable<SubscriberConfiguration>> MapCosmosEntries()
            {
                var tasks = subscribersFromCosmos.Select(this.MapSubscriber).ToArray();
                await Task.WhenAll(tasks);

                return tasks.SelectMany(t => t.Result).ToArray();
            }

            var fromCosmos = await MapCosmosEntries();
            var union = onlyInKv.Union(fromCosmos).ToList();
            return new ReadOnlyCollection<SubscriberConfiguration>(union);
        }

        private async Task<IEnumerable<SubscriberConfiguration>> MapSubscriber(SubscriberEntity cosmosModel)
        {
            return new[]
            {
                await MapBasicSubscriberData(cosmosModel)
            };

            // DLQ handling not needed now
            //var dlq = cosmosModel?.Dlq?.Endpoints.FirstOrDefault();
            //if (dlq != null)
            //{
            //    yield return MapDlq(cosmosModel);
            //}
        }

        private async Task<SubscriberConfiguration> MapBasicSubscriberData(SubscriberEntity cosmosModel)
        {
            var config = new SubscriberConfiguration
            {
                Name = cosmosModel.ParentEvent.Name,
                SubscriberName = cosmosModel.Name,
                Uri = cosmosModel.Webhooks.Endpoints.First().Uri,
                AuthenticationConfig = await MapAuthentication(cosmosModel.Webhooks.Endpoints.FirstOrDefault()?.Authentication),
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

        private async Task<AuthenticationConfig> MapAuthentication(AuthenticationEntity cosmosAuthentication)
        {
            if (cosmosAuthentication?.SecretStore?.SecretName == null)
                return null;

            var secretValue = await _secretManager.GetSecretValueAsync(cosmosAuthentication.SecretStore.SecretName);

            return new OidcAuthenticationConfig
            {
                Type = AuthenticationType.OIDC,
                ClientId = cosmosAuthentication.ClientId,
                ClientSecret = secretValue,
                Uri = cosmosAuthentication.Uri,
                Scopes = cosmosAuthentication.Scopes,
            };
        }
    }
}
