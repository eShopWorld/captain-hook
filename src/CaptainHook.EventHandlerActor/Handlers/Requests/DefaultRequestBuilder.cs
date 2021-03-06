using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry.Message;
using Eshopworld.Core;
using Eshopworld.Platform.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptainHook.EventHandlerActor.Handlers.Requests
{
    public class DefaultRequestBuilder : IRequestBuilder
    {
        protected readonly IBigBrother BigBrother;

        public DefaultRequestBuilder(IBigBrother bigBrother)
        {
            BigBrother = bigBrother;
        }

        /// <inheritdoc />
        public Uri BuildUri(WebhookConfig config, string payload)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return BuildUriFromExistingConfig(config, payload);
        }

        protected virtual Uri BuildUriFromExistingConfig(WebhookConfig config, string payload)
        {
            var uri = config.Uri;

            //build the uri from the routes first
            var rules = config.WebhookRequestRules.FirstOrDefault(r => r.Destination.RuleAction == RuleAction.Route);
            if (rules != null)
            {
                var selector = string.Empty;
                if (rules.Source.Location == Location.Body)
                {
                    selector = ModelParser.ParsePayloadPropertyAsString(rules.Source.Path, payload);
                }

                if (string.IsNullOrWhiteSpace(selector))
                {
                    this.PublishUnroutableEvent(config, "routing path value in message payload is null or empty");
                    return null;
                }

                //selects the route based on the value found in the payload of the message
                var route = rules.Routes.FirstOrDefault(r => r.Selector.Equals(selector, StringComparison.OrdinalIgnoreCase));
                if (route == null)
                {
                    this.PublishUnroutableEvent(config, "route mapping/selector not found between config and the properties on the domain object", selector);
                    return null;
                }
                uri = route.Uri;
            }

            void PublishUnroutableEventWithMessage(string message) => PublishUnroutableEvent(config, message);

            //after route has been selected then select the identifier for the RESTful URI if applicable
            var uriRules = config.WebhookRequestRules.FirstOrDefault(l => l.Destination.Location == Location.Uri);
            if (uriRules == null)
            {
                return BuildUriContext.CreateUriOrPublishUnroutable(uri, PublishUnroutableEventWithMessage);
            }

            if (uriRules.Source.Location != Location.Body)
            {
                return BuildUriContext.CreateUriOrPublishUnroutable(uri, PublishUnroutableEventWithMessage);
            }

            var parameter = ModelParser.ParsePayloadPropertyAsString(uriRules.Source.Path, payload);
            uri = CombineUriAndResourceId(uri, parameter);
            return BuildUriContext.CreateUriOrPublishUnroutable(uri, PublishUnroutableEventWithMessage);
        }

        protected void PublishUnroutableEvent(WebhookConfig config, string message, string selector = null)
        {
            BigBrother.Publish(new UnroutableMessageEvent
            {
                EventType = config.EventType,
                Selector = selector,
                SubscriberName = config.Name,
                Message = message
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private static string CombineUriAndResourceId(string uri, string parameter)
        {
            var position = uri.LastIndexOfSafe('/');
            var encodedParameter = Uri.EscapeDataString(parameter);
            uri = position == uri.Length - 1 ? $"{uri}{encodedParameter}" : $"{uri}/{encodedParameter}";
            return uri;
        }

        /// <inheritdoc />
        public string BuildPayload(WebhookConfig config, string sourcePayload, IDictionary<string, object> metadata)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var rules = config.WebhookRequestRules.Where(l => l.Destination.Location == Location.Body).ToList();

            if (!rules.Any())
            {
                return sourcePayload;
            }

            //Any replace action replaces the payload 
            var replaceRule = rules.FirstOrDefault(r => r.Destination.RuleAction == RuleAction.Replace);
            if (replaceRule != null)
            {
                var destinationPayload = ModelParser.ParsePayloadProperty(replaceRule.Source, sourcePayload, replaceRule.Destination.Type);

                if (rules.Count <= 1)
                {
                    return destinationPayload.ToString(Formatting.None);
                }
            }

            if (metadata == null)
            {
                metadata = new Dictionary<string, object>();
            }

            JContainer payload = new JObject();
            foreach (var rule in rules)
            {
                if (rule.Destination.RuleAction != RuleAction.Add)
                {
                    continue;
                }

                if (rule.Source.RuleAction == RuleAction.Route)
                {
                    continue;
                }

                object value;
                switch (rule.Source.Type)
                {
                    case DataType.String:
                    case DataType.Property:
                    case DataType.Model:
                        value = ModelParser.ParsePayloadProperty(rule.Source, sourcePayload, rule.Destination.Type);
                        break;

                    case DataType.HttpContent:
                        metadata.TryGetValue("HttpResponseContent", out var httpContent);
                        value = ModelParser.GetJObject(httpContent, rule.Destination.Type);
                        break;

                    case DataType.HttpStatusCode:
                        metadata.TryGetValue("HttpStatusCode", out var httpStatusCode);
                        value = ModelParser.GetJObject(httpStatusCode, rule.Destination.Type);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (string.IsNullOrWhiteSpace(rule.Destination.Path))
                {
                    payload = (JContainer)value;
                    continue;
                }

                payload.Add(new JProperty(rule.Destination.Path, value));
            }

            return payload.ToString(Formatting.None);
        }

        /// <inheritdoc />

        public virtual HttpMethod SelectHttpMethod(WebhookConfig webhookConfig, string payload)
        {
            if (webhookConfig == null) throw new ArgumentNullException(nameof(webhookConfig));

            //build the uri from the routes first
            var rules = webhookConfig.WebhookRequestRules.FirstOrDefault(r => r.Destination.RuleAction == RuleAction.Route);
            if (rules == null)
            {
                return webhookConfig.HttpMethod;
            }

            var value = ModelParser.ParsePayloadPropertyAsString(rules.Source.Path, payload);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(rules.Source.Path), "routing path value in message payload is null or empty");
            }

            var route = rules.Routes.FirstOrDefault(r => r.Selector.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (route == null)
            {
                throw new Exception("route http verb mapping/selector not found between config and the properties on the domain object");
            }

            return route.HttpMethod;
        }

        /// <inheritdoc />
        public virtual WebhookConfig GetAuthenticationConfig(WebhookConfig webhookConfig, string payload)
        {
            if (webhookConfig == null) throw new ArgumentNullException(nameof(webhookConfig));

            //build the uri from the routes first
            var rules = webhookConfig.WebhookRequestRules.FirstOrDefault(r => r.Destination.RuleAction == RuleAction.Route);
            if (rules == null)
            {
                return webhookConfig;
            }

            var value = ModelParser.ParsePayloadPropertyAsString(rules.Source.Path, payload);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(rules.Source.Path), "routing path value in message payload is null or empty");
            }

            var route = rules.Routes.FirstOrDefault(r => r.Selector.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (route == null)
            {
                throw new InvalidOperationException("route http verb mapping/selector not found between config and the properties on the domain object");
            }
            return route;
        }

        /// <inheritdoc />
        public WebhookConfig SelectWebhookConfig(WebhookConfig webhookConfig, string payload)
        {
            if (webhookConfig == null) throw new ArgumentNullException(nameof(webhookConfig));

            return SelectWebhookConfigCore(webhookConfig, payload);
        }

        protected virtual WebhookConfig SelectWebhookConfigCore(WebhookConfig webhookConfig, string payload)
        {
            var rules = webhookConfig.WebhookRequestRules.FirstOrDefault(r => r.Destination.RuleAction == RuleAction.Route);
            if (rules == null)
            {
                return webhookConfig;
            }

            var selector = string.Empty;
            if (rules.Source.Location == Location.Body)
            {
                selector = ModelParser.ParsePayloadPropertyAsString(rules.Source.Path, payload);
            }

            if (string.IsNullOrWhiteSpace(selector))
            {
                throw new InvalidOperationException($"routing path value in message payload is null or empty: {nameof(rules.Source.Path)}");
            }

            //selects the route based on the value found in the payload of the message
            var route = rules.Routes.FirstOrDefault(r => r.Selector.Equals(selector, StringComparison.OrdinalIgnoreCase));
            if (route == null)
            {
                throw new InvalidOperationException("route mapping/selector not found between config and the properties on the domain object");
            }

            return route;
        }

        /// <inheritdoc />
        public WebHookHeaders GetHttpHeaders(WebhookConfig webhookConfig, MessageData messageData)
        {
            if (webhookConfig == null) throw new ArgumentNullException(nameof(webhookConfig));
            if (messageData == null) throw new ArgumentNullException(nameof(messageData));

            var headers = new WebHookHeaders();

            headers.AddContentHeader(Constants.Headers.ContentType, webhookConfig.ContentType);
            headers.AddRequestHeader(Constants.Headers.CorrelationId, messageData.CorrelationId);
            headers.AddRequestHeader(Constants.Headers.EventDeliveryId, messageData.CorrelationId);
            headers.AddRequestHeader(Constants.Headers.EventType, webhookConfig.PayloadTransformation==PayloadContractTypeEnum.WrapperContract ? typeof(NewtonsoftDeliveryStatusMessage).FullName.ToLowerInvariant(): webhookConfig.EventType);
            headers.AddRequestHeader(Constants.Headers.IdempotencyKey, messageData.ServiceBusMessageId);

            return headers;
        }
    }
}
