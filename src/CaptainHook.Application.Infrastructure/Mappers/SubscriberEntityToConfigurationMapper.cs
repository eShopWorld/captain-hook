using System;
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
        private static readonly Dictionary<string, string> EmptyReplacementsDictionary = new Dictionary<string, string>();

        private static readonly WebhookRequestRule StatusCodeRequestRule = new WebhookRequestRule
        {
            Source = new SourceParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.HttpStatusCode
            },
            Destination = new ParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.Property,
                Path = "StatusCode"
            }
        };

        private static readonly WebhookRequestRule ContentRequestRule = new WebhookRequestRule
        {
            Source = new SourceParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.HttpContent
            },
            Destination = new ParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.String,
                Path = "Content"
            }
        };

        private static readonly List<WebhookRequestRule> CallbackRequestRules = new List<WebhookRequestRule>
        {
            StatusCodeRequestRule,
            ContentRequestRule
        };

        private readonly ISecretProvider _secretProvider;

        public SubscriberEntityToConfigurationMapper(ISecretProvider secretProvider)
        {
            _secretProvider = secretProvider;
        }

        public async Task<OperationResult<SubscriberConfiguration>> MapToWebhookAsync(SubscriberEntity entity)
        {
            var webhooksResult = await MapWebhooksAsync(entity.Id, entity.ParentEvent.Name, entity.Webhooks);
            if (webhooksResult.IsError)
            {
                return webhooksResult.Error;
            }

            var subscriberConfiguration = SubscriberConfiguration.FromWebhookConfig(webhooksResult.Data);
            subscriberConfiguration.SubscriberName = entity.Name;

            if (entity.HasCallbacks)
            {
                var callbackResult = await MapWebhooksAsync(entity.Id, entity.ParentEvent.Name, entity.Callbacks);
                if (callbackResult.IsError)
                {
                    return callbackResult.Error;
                }
                subscriberConfiguration.Callback = callbackResult.Data;
                AddCallbackRules(subscriberConfiguration.Callback);
            }

            return subscriberConfiguration;
        }

        public async Task<OperationResult<SubscriberConfiguration>> MapToDlqAsync(SubscriberEntity entity)
        {
            if (!entity.HasDlqHooks)
                throw new ArgumentException("Entity must contain Dlqhooks", nameof(entity));

            var webhooksResult = await MapWebhooksAsync(entity.Id, entity.ParentEvent.Name, entity.DlqHooks);
            if (webhooksResult.IsError)
            {
                return webhooksResult.Error;
            }

            var subscriberConfiguration = SubscriberConfiguration.FromWebhookConfig(webhooksResult.Data);
            subscriberConfiguration.DLQMode = SubscriberDlqMode.WebHookMode;
            subscriberConfiguration.SourceSubscriptionName = entity.Name;
            subscriberConfiguration.SubscriberName = $"{entity.Name}-DLQ";

            return subscriberConfiguration;
        }

        private void AddCallbackRules(WebhookConfig callback)
        {
            if (callback.WebhookRequestRules == null)
            {
                callback.WebhookRequestRules = new List<WebhookRequestRule>();
            }

            callback.WebhookRequestRules.AddRange(CallbackRequestRules);
        }

        private async Task<OperationResult<WebhookConfig>> MapWebhooksAsync(string name, string eventType, WebhooksEntity webhooksEntity)
        {
            if (string.IsNullOrEmpty(webhooksEntity?.SelectionRule) &&
                webhooksEntity?.Endpoints?.Count() == 1 &&
                webhooksEntity?.UriTransform == null)
            {
                return await MapSingleWebhookWithNoUriTransformAsync(name, eventType, webhooksEntity);
            }
            else
            {
                return await MapForUriTransformAsync(name, eventType, webhooksEntity);
            }
        }

        private async Task<OperationResult<WebhookConfig>> MapSingleWebhookWithNoUriTransformAsync(string name, string eventType, WebhooksEntity webhooksEntity)
        {
            var authenticationResult = await MapAuthenticationAsync(webhooksEntity?.Endpoints?.FirstOrDefault()?.Authentication);

            if (authenticationResult.IsError)
            {
                return authenticationResult.Error;
            }

            return new WebhookConfig
            {
                Name = name,
                EventType = eventType,
                Uri = webhooksEntity?.Endpoints?.FirstOrDefault()?.Uri,
                HttpVerb = webhooksEntity?.Endpoints?.FirstOrDefault()?.HttpVerb,
                AuthenticationConfig = authenticationResult.Data,
            };
        }

        private async Task<OperationResult<WebhookConfig>> MapForUriTransformAsync(string name, string eventType, WebhooksEntity webhooksEntity)
        {
            var replacements = new Dictionary<string, string>(webhooksEntity.UriTransform?.Replace ?? EmptyReplacementsDictionary);

            if (!string.IsNullOrEmpty(webhooksEntity.SelectionRule))
            {
                replacements["selector"] = webhooksEntity.SelectionRule;
            }

            var routesResult = await MapWebhooksToRoutesAsync(webhooksEntity);

            if (routesResult.IsError)
            {
                return routesResult.Error;
            }

            return new WebhookConfig
            {
                Name = name,
                EventType = eventType,
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

            if (authenticationResult.IsError)
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
    }
}