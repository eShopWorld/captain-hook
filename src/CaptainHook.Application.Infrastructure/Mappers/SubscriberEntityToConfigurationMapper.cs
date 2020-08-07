using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Domain.Entities;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public class SubscriberEntityToConfigurationMapper : ISubscriberEntityToConfigurationMapper
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
                await MapBasicSubscriberData(cosmosModel)
            };

            // DLQ handling not needed now
            //var dlq = cosmosModel?.Dlq?.Endpoints.FirstOrDefault();
            //if (dlq != null)
            //{
            //    yield return MapDlq(cosmosModel);
            //}
        }

        private async Task<SubscriberConfiguration> MapBasicSubscriberData(SubscriberEntity entity)
        {
            SubscriberConfiguration config;

            var firstEndpoint = entity.Webhooks?.Endpoints?.FirstOrDefault();
            if (firstEndpoint?.UriTransform != null)
            {
                config = await MapForUriTransform(entity, firstEndpoint);
            }
            else
            {
                config = await MapStandardWay(entity, firstEndpoint);
            }

            return config;
        }

        private async Task<SubscriberConfiguration> MapStandardWay(SubscriberEntity entity, EndpointEntity firstEndpoint)
        {
            return new SubscriberConfiguration
            {
                Name = entity.Id,
                SubscriberName = entity.Name,
                EventType = entity.ParentEvent.Name,
                Uri = firstEndpoint.Uri,
                AuthenticationConfig = await MapAuthentication(firstEndpoint.Authentication),
            };
        }

        private async Task<SubscriberConfiguration> MapForUriTransform(SubscriberEntity entity, EndpointEntity firstEndpoint)
        {
            return new SubscriberConfiguration
            {
                Name = entity.Id,
                SubscriberName = entity.Name,
                EventType = entity.ParentEvent.Name,
                WebhookRequestRules = new List<WebhookRequestRule>
                {
                    new WebhookRequestRule
                    {
                        Source = new SourceParserLocation {Replace = firstEndpoint.UriTransform.Replace},
                        Destination = new ParserLocation {RuleAction = RuleAction.RouteAndReplace},
                        Routes = new List<WebhookConfigRoute>
                        {
                            new WebhookConfigRoute
                            {
                                Uri = firstEndpoint.Uri,
                                HttpVerb = firstEndpoint.HttpVerb,
                                Selector = "*",
                                AuthenticationConfig = await MapAuthentication(firstEndpoint.Authentication),
                            }
                        }
                    }
                }
            };
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