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

        private static WebhookRequestRule PayloadTransformRule(string payloadTransform) => new WebhookRequestRule
        {
            Source = new SourceParserLocation
            {
                Path = payloadTransform,
                Type = DataType.Model
            },
            Destination = new ParserLocation
            {
                Type = DataType.Model
            }
        };

        private readonly ISecretProvider _secretProvider;

        public SubscriberEntityToConfigurationMapper(ISecretProvider secretProvider)
        {
            _secretProvider = secretProvider;
        }

        public async Task<OperationResult<SubscriberConfiguration>> MapToWebhookAsync(SubscriberEntity entity)
        {
            var webhooksResult = await MapWebhooksAsync(entity, entity.Webhooks);
            if (webhooksResult.IsError)
            {
                return webhooksResult.Error;
            }

            var subscriberConfiguration = SubscriberConfiguration.FromWebhookConfig(webhooksResult.Data);
            subscriberConfiguration.SubscriberName = entity.Name;

            if (entity.HasCallbacks)
            {
                var callbackResult = await MapWebhooksAsync(entity, entity.Callbacks);
                if (callbackResult.IsError)
                {
                    return callbackResult.Error;
                }
                subscriberConfiguration.Callback = callbackResult.Data;
                subscriberConfiguration.Callback.WebhookRequestRules.AddRange(CallbackRequestRules);
            }

            return subscriberConfiguration;
        }

        public async Task<OperationResult<SubscriberConfiguration>> MapToDlqAsync(SubscriberEntity entity)
        {
            if (!entity.HasDlqHooks)
                throw new ArgumentException("Entity must contain Dlqhooks", nameof(entity));

            var webhooksResult = await MapWebhooksAsync(entity, entity.DlqHooks);
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

        private async Task<OperationResult<WebhookConfig>> MapWebhooksAsync(SubscriberEntity entity, WebhooksEntity webhooksEntity)
        {
            OperationResult<WebhookConfig> result;

            if (string.IsNullOrEmpty(webhooksEntity?.SelectionRule) &&
                webhooksEntity?.Endpoints?.Count == 1 &&
                webhooksEntity?.UriTransform == null)
            {
                result = await MapSingleWebhookWithNoUriTransformAsync(entity, webhooksEntity);
            }
            else
            {
                result = await MapForUriTransformAsync(entity, webhooksEntity);
            }

            return result.Then<WebhookConfig>(config => AddPayloadTransformRule(config, webhooksEntity.PayloadTransform, webhooksEntity.Type));
        }

        private WebhookConfig AddPayloadTransformRule(WebhookConfig webhookConfig, string payloadTransform, WebhooksEntityType entityType)
        {
            if (entityType != WebhooksEntityType.Callbacks)
            {
                webhookConfig.WebhookRequestRules.Add(PayloadTransformRule(payloadTransform));
            }

            return webhookConfig;
        }

        private async Task<OperationResult<WebhookConfig>> MapSingleWebhookWithNoUriTransformAsync(SubscriberEntity entity, WebhooksEntity webhooksEntity)
        {
            var name = entity.Id;
            var eventType = entity.ParentEvent.Name;

            var authenticationResult = await MapAuthenticationAsync(webhooksEntity?.Endpoints?.FirstOrDefault()?.Authentication);

            if (authenticationResult.IsError)
            {
                return authenticationResult.Error;
            }

            var config = new WebhookConfig
            {
                Name = name,
                EventType = eventType,
                Uri = webhooksEntity?.Endpoints?.FirstOrDefault()?.Uri,
                HttpVerb = webhooksEntity?.Endpoints?.FirstOrDefault()?.HttpVerb,
                AuthenticationConfig = authenticationResult.Data
            };

            if (entity.MaxDeliveryCount.HasValue)
            {
                config.MaxDeliveryCount = entity.MaxDeliveryCount.Value;
            }

            return config;
        }

        private async Task<OperationResult<WebhookConfig>> MapForUriTransformAsync(SubscriberEntity entity, WebhooksEntity webhooksEntity)
        {
            var name = entity.Id;
            var eventType = entity.ParentEvent.Name;

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

            var config = new WebhookConfig
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
                },
            };

            if (entity.MaxDeliveryCount.HasValue)
            {
                config.MaxDeliveryCount = entity.MaxDeliveryCount.Value;
            }

            return config;
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

            var route = new WebhookConfigRoute
            {
                Uri = endpoint.Uri,
                HttpVerb = endpoint.HttpVerb,
                Selector = endpoint.Selector ?? "*",
                AuthenticationConfig = authenticationResult.Data
            };

            if (endpoint.RetrySleepDurations != null)
            {
                route.RetrySleepDurations = endpoint.RetrySleepDurations;
            }

            if (endpoint.Timeout.HasValue)
            {
                route.Timeout = endpoint.Timeout.Value;
            }

            return route;
        }

        private async Task<OperationResult<AuthenticationConfig>> MapAuthenticationAsync(AuthenticationEntity authenticationEntity)
        {
            return authenticationEntity switch
            {
                OidcAuthenticationEntity ent => (await MapOidcAuthenticationAsync(ent)).Then<AuthenticationConfig>(x => x),
                BasicAuthenticationEntity ent => (await MapBasicAuthenticationAsync(ent)).Then<AuthenticationConfig>(x => x),
                null => new AuthenticationConfig(),
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