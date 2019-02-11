using System;
using System.Linq;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Nasty;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class RequestBuilder
    {
        public static string BuildUri(WebhookConfig config, string payload)
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

        private static string CombineUriAndResourceId(string uri, string parameter)
        {
            var position = uri.LastIndexOfSafe('/');
            uri = position == uri.Length - 1 ? $"{uri}{parameter}" : $"{uri}/{parameter}";
            return uri;
        }

        public static string BuildPayload(WebhookConfig config, string payload)
        {
            return string.Empty;
        }

        public static (string uri, string payload) BuildRequest(WebhookConfig config, string payload)
        {
            return (string.Empty, string.Empty);
        }
    }
}