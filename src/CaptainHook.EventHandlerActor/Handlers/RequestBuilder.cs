using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Nasty;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class RequestBuilder : IRequestBuilder
    {
        public string BuildUri(WebhookConfig config, string payload)
        {
            var uri = string.Empty;

            //build the uri from the routes first
            var routingRules = config.WebhookRequestRules.FirstOrDefault(l => l.Routes.Any());
            if (routingRules != null)
            {
                if (routingRules.Source.Location == Location.MessageBody)
                {
                    var path = routingRules.Source.Path;
                    var value = ModelParser.ParsePayloadPropertyAsString(path, payload);

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentNullException(nameof(path), "routing path value in message payload is null or empty");
                    }

                    WebhookConfigRoute route = null;
                    foreach (var rules in config.WebhookRequestRules)
                    {
                        route = rules.Routes.FirstOrDefault(r => r.Selector.Equals(value, StringComparison.OrdinalIgnoreCase));
                        if (route != null)
                        {
                            break;
                        }
                    }

                    if (route != null)
                    {
                        uri = route.Uri;
                    }
                }
            }

            //after route has been selected then select the identifier for the RESTful URI if applicable
            var uriRules = config.WebhookRequestRules.FirstOrDefault(l => l.Destination.Location == Location.Uri);
            if (uriRules != null)
            {
                if (uriRules.Source.Location == Location.MessageBody)
                {
                    var parameter = ModelParser.ParsePayloadPropertyAsString(uriRules.Source.Path, payload);

                    if (uri == string.Empty)
                    {
                        uri = config.Uri;
                    }
                    uri = CombineUriAndResourceId(uri, parameter);
                }
            }
            return uri;
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
            uri = position == uri.Length - 1 ? $"{uri}{parameter}" : $"{uri}/{parameter}";
            return uri;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="sourcePayload"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string BuildPayload(WebhookConfig config, string sourcePayload, Dictionary<string, string> data)
        {
            var rules = config.WebhookRequestRules.Where(l => l.Destination.Location == Location.PayloadBody).ToList();

            if (!rules.Any())
            {
                return null;
            }
            
            //Any replace action replaces the payload 
            var replaceRule = rules.FirstOrDefault(r => r.Destination.RuleAction == RuleAction.Replace);

            if (replaceRule != null)
            {
                var destinationPayload = ModelParser.ParsePayloadPropertyAsString(replaceRule.Source, sourcePayload);
                
                if (rules.Count <= 1)
                {
                    return destinationPayload;
                }
            }

            var payload = new JObject();
            foreach (var rule in rules)
            {
                if (rule.Destination.RuleAction == RuleAction.Add)
                {
                    JToken value;
                    switch (rule.Source.Type)
                    {
                        case DataType.Property:
                        case DataType.Model:
                            value = ModelParser.ParsePayloadProperty(rule.Source, sourcePayload);
                            break;
                            
                        case DataType.HttpContent:
                            value = ModelParser.GetJObject(data["HttpResponseContent"]);
                            break;

                        case DataType.HttpStatusCode:
                            value = data["HttpStatusCode"];
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    ModelParser.AddPropertyToPayload(rule.Destination, value, payload);
                }
            }
            
            return payload.ToString(Formatting.None);
        }
    }
}