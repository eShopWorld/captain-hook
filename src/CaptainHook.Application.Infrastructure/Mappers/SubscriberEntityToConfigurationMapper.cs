using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Domain.Entities;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public class SubscriberEntityToConfigurationMapper
    {
        private readonly ISecretProvider _secretProvider;

        public SubscriberEntityToConfigurationMapper(ISecretProvider secretProvider)
        {
            _secretProvider = secretProvider;
        }

        public async Task<IEnumerable<SubscriberConfiguration>> MapSubscriber(SubscriberEntity cosmosModel)
        {
            return new[]
            {
                await MapSingleSubscriber(cosmosModel)
            };

            // DLQ handling not needed now
            //var dlq = cosmosModel?.Dlq?.Endpoints.FirstOrDefault();
            //if (dlq != null)
            //{
            //    yield return MapDlq(cosmosModel);
            //}
        }

        public async Task<SubscriberConfiguration> MapSingleSubscriber(SubscriberEntity cosmosModel)
        {
            var config = new SubscriberConfiguration
            {
                Name = $"{cosmosModel.ParentEvent.Name}-{cosmosModel.Name}",
                SubscriberName = cosmosModel.Name,
                EventType = cosmosModel.ParentEvent.Name,
                Uri = cosmosModel.Webhooks.Endpoints.First().Uri,
                AuthenticationConfig = await MapAuthentication(cosmosModel.Webhooks.Endpoints.FirstOrDefault()?.Authentication),
                // Callback handling not needed now
                //Callback = MapCallback(cosmosModel),
            };

            return config;
        }

        private async Task<AuthenticationConfig> MapAuthentication(AuthenticationEntity cosmosAuthentication)
        {
            if (cosmosAuthentication?.SecretStore?.SecretName == null)
                return null;

            var secretValue = await _secretProvider.GetSecretValueAsync(cosmosAuthentication.SecretStore.SecretName);

            return new OidcAuthenticationConfig
            {
                Type = AuthenticationType.OIDC,
                ClientId = cosmosAuthentication.ClientId,
                ClientSecret = secretValue,
                Uri = cosmosAuthentication.Uri,
                Scopes = cosmosAuthentication.Scopes,
            };
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
    }
}