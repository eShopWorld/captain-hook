using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Infrastructure.Mappers
{
    public class SubscriberEntityToConfigurationMapper : ISubscriberEntityToConfigurationMapper
    {
        private readonly ISecretProvider _secretProvider;

        public SubscriberEntityToConfigurationMapper(ISecretProvider secretProvider)
        {
            _secretProvider = secretProvider;
        }

        public async Task<OperationResult<IEnumerable<SubscriberConfiguration>>> MapSubscriberAsync(SubscriberEntity entity)
        {
            var webhooksResult = await MapWebhooksAsync(entity);

            if (webhooksResult.IsError)
            {
                return webhooksResult.Error;
            }

            /*
            var callbacksResult = await MapCallbacksAsync(entity);

            if (callbacksResult.IsError)
            {
                return callbacksResult.Error;
            }

            var dlqResult = await MapDlqAsync(entity);

            if (dlqResult.IsError)
            {
                return dlqResult.Error;
            }
            */

            return new[] { webhooksResult.Data, /* callbacksResult, dlqResult */ };
        }

        private async Task<OperationResult<SubscriberConfiguration>> MapWebhooksAsync(SubscriberEntity entity)
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

        private async Task<OperationResult<SubscriberConfiguration>> MapSingleWebhookWithNoUriTransformAsync(SubscriberEntity entity)
        {
            var authenticationResult = await MapAuthenticationAsync(entity.Webhooks?.Endpoints?.FirstOrDefault()?.Authentication);

            if(authenticationResult.IsError)
            {
                return authenticationResult.Error;
            }

            return new SubscriberConfiguration
            {
                Name = entity.Id,
                SubscriberName = entity.Name,
                EventType = entity.ParentEvent.Name,
                Uri = entity.Webhooks?.Endpoints?.FirstOrDefault()?.Uri,
                HttpVerb = entity.Webhooks?.Endpoints?.FirstOrDefault()?.HttpVerb,
                AuthenticationConfig = authenticationResult.Data,
            };
        }

        private async Task<OperationResult<SubscriberConfiguration>> MapForUriTransformAsync(SubscriberEntity entity)
        {
            var replacements = entity.Webhooks.UriTransform?.Replace ?? new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(entity.Webhooks.SelectionRule))
            {
                replacements["selector"] = entity.Webhooks.SelectionRule;
            }

            var routesResult = await MapWebhooksToRoutesAsync(entity.Webhooks);

            if(routesResult.IsError)
            {
                return routesResult.Error;
            }

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
                        Routes = routesResult.Data.ToList()
                    }
                }
            };
        }

        private async Task<OperationResult<IEnumerable<WebhookConfigRoute>>> MapWebhooksToRoutesAsync(WebhooksEntity webhooks)
        {
            var tasks = webhooks.Endpoints.Select(MapEndpointToRouteAsync);
            await Task.WhenAll(tasks);

            var errors = tasks.Select(x => x.Result).Where(x => x.IsError);

            if (errors.Any())
            {
                var failures = errors.Select(x => new Failure("Mapping", x.Error.Message)).ToArray();
                return new MappingError("Cannot translate webhook to routes", failures);
            }

            return tasks.Select(x => x.Result.Data).ToList();
        }

        private async Task<OperationResult<WebhookConfigRoute>> MapEndpointToRouteAsync(EndpointEntity endpoint)
        {
            var authenticationResult = await MapAuthenticationAsync(endpoint.Authentication);

            if(authenticationResult.IsError)
            {
                return authenticationResult.Error;
            }

            return new WebhookConfigRoute
            {
                Uri = endpoint.Uri,
                HttpVerb = endpoint.HttpVerb,
                Selector = endpoint.Selector ?? "*",
                AuthenticationConfig = authenticationResult.Data
            };
        }

        private async Task<OperationResult<AuthenticationConfig>> MapAuthenticationAsync(AuthenticationEntity authenticationEntity)
        {
            return authenticationEntity switch
            {
                OidcAuthenticationEntity ent => (await MapOidcAuthenticationAsync(ent)).Then<AuthenticationConfig>(x => x),
                BasicAuthenticationEntity ent => (await MapBasicAuthenticationAsync(ent)).Then<AuthenticationConfig>(x => x),
                _ => new MappingError("Could not find a suitable authentication mechanism.")
            };
        }

        private async Task<OperationResult<OidcAuthenticationConfig>> MapOidcAuthenticationAsync(OidcAuthenticationEntity authenticationEntity)
        {
            if (string.IsNullOrWhiteSpace(authenticationEntity?.ClientSecretKeyName))
            {
                return new MappingError("Cannot retrieve Client Secret from an invalid Key Name.");
            }

            try
            {
                var secretValue = await _secretProvider.GetSecretValueAsync(authenticationEntity.ClientSecretKeyName);

                return new OidcAuthenticationConfig
                {
                    ClientId = authenticationEntity.ClientId,
                    ClientSecret = secretValue,
                    Uri = authenticationEntity.Uri,
                    Scopes = authenticationEntity.Scopes,
                };
            }
            catch
            {
                return new MappingError($"Cannot retrieve Client Secret from Key Name '{authenticationEntity.ClientSecretKeyName}'.");
            }

        }

        private async Task<OperationResult<BasicAuthenticationConfig>> MapBasicAuthenticationAsync(BasicAuthenticationEntity authenticationEntity)
        {
            if (string.IsNullOrWhiteSpace(authenticationEntity?.PasswordKeyName))
            {
                return new MappingError("Cannot retrieve Password from an invalid Key Name.");
            }

            try
            { 
                var secretValue = await _secretProvider.GetSecretValueAsync(authenticationEntity.PasswordKeyName);

                return new BasicAuthenticationConfig
                {
                    Username = authenticationEntity.Username,
                    Password = secretValue
                };
            }
            catch
            {
                return new MappingError($"Cannot retrieve Password from a Key Name '{authenticationEntity.PasswordKeyName}'.");
            }
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