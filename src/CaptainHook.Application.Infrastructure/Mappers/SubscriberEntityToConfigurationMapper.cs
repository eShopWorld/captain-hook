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

        public async Task<IEnumerable<SubscriberConfiguration>> MapSubscriberAsync(SubscriberEntity entity)
        {
            return new []
            {
                await MapWebhooksAsync(entity)
                // await MapCallbacksAsync(entity)
                // await MapDlqsAsync(entity)
            };
        }

        private async Task<SubscriberConfiguration> MapWebhooksAsync(SubscriberEntity entity)
        {
            if (string.IsNullOrEmpty(entity.Webhooks?.SelectionRule) &&
                entity.Webhooks?.Endpoints?.Count() == 1 &&
                entity.Webhooks?.UriTransform == null)
            {
                return await MapSingleWebhookWithNoUriTransformAsync(entity);
            }
            else
            {
                return await MapForUriTransformAsync(entity);
            }
        }

        private async Task<SubscriberConfiguration> MapSingleWebhookWithNoUriTransformAsync(SubscriberEntity entity)
        {
            return new SubscriberConfiguration
            {
                Name = entity.Id,
                SubscriberName = entity.Name,
                EventType = entity.ParentEvent.Name,
                Uri = entity.Webhooks?.Endpoints?.FirstOrDefault()?.Uri,
                HttpVerb = entity.Webhooks?.Endpoints?.FirstOrDefault()?.HttpVerb,
                AuthenticationConfig = await MapAuthenticationAsync(entity.Webhooks?.Endpoints?.FirstOrDefault()?.Authentication),
            };
        }

        private async Task<SubscriberConfiguration> MapForUriTransformAsync(SubscriberEntity entity)
        {
            var replacements = entity.Webhooks.UriTransform?.Replace ?? new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(entity.Webhooks.SelectionRule))
            {
                replacements["selector"] = entity.Webhooks.SelectionRule;
            }

            var routes = await MapWebhooksToRoutesAsync(entity.Webhooks);

            return new SubscriberConfiguration
            {
                Name = entity.Id,
                SubscriberName = entity.Name,
                EventType = entity.ParentEvent.Name,
                WebhookRequestRules = new List<WebhookRequestRule>
                {
                    new WebhookRequestRule
                    {
                        Source = new SourceParserLocation { Replace = replacements },
                        Destination = new ParserLocation { RuleAction = RuleAction.RouteAndReplace },
                        Routes = routes.ToList()
                    }
                }
            };
        }

        private async Task<IEnumerable<WebhookConfigRoute>> MapWebhooksToRoutesAsync(WebhooksEntity webhooks)
        {
            var tasks = webhooks.Endpoints.Select(MapEndpointToRouteAsync);
            return await Task.WhenAll(tasks);
        }

        private async Task<WebhookConfigRoute> MapEndpointToRouteAsync(EndpointEntity endpoint)
        {
            return new WebhookConfigRoute
            {
                Uri = endpoint.Uri,
                HttpVerb = endpoint.HttpVerb,
                Selector = endpoint.Selector ?? "*",
                AuthenticationConfig = await MapAuthenticationAsync(endpoint.Authentication),
            };
        }

        private async Task<AuthenticationConfig> MapAuthenticationAsync(AuthenticationEntity cosmosAuthentication)
        {
            if (cosmosAuthentication?.ClientSecretKeyName == null)
                return null;

            var secretValue = await _secretProvider.GetSecretValueAsync(cosmosAuthentication.ClientSecretKeyName);

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