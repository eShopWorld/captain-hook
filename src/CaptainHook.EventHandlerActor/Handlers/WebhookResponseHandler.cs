using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Nasty;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public static class StringExtensions
    {

        public static int LastIndexOfSafe(this string value, char searchCharacter)
        {
            try
            {
                var position = value.LastIndexOf("/", StringComparison.Ordinal);
                return position;
            }
            catch
            {
                // ignored
            }

            return 0;
        }
    }


    public class RequestBuilder
    {
        public static string BuildUri(WebhookConfig config, string payload)
        {
            var uri = string.Empty;

            //build the uri from the routes first
            var routingRules = config.WebhookRequestRules.FirstOrDefault(l => l.Routes.Any());
            if (routingRules != null)
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
                    uri = DoIt(uri, parameter);
                }
            }
            return uri;
        }

        private static string DoIt(string uri, string parameter)
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

    public class WebhookResponseHandler : GenericWebhookHandler
    {
        private readonly HttpClient _client;
        private readonly EventHandlerConfig _eventHandlerConfig;
        private readonly IEventHandlerFactory _eventHandlerFactory;

        public WebhookResponseHandler(
            IEventHandlerFactory eventHandlerFactory,
            IAcquireTokenHandler acquireTokenHandler,
            IBigBrother bigBrother,
            HttpClient client,
            EventHandlerConfig eventHandlerConfig)
            : base(acquireTokenHandler, bigBrother, client, eventHandlerConfig.WebHookConfig)
        {
            _eventHandlerFactory = eventHandlerFactory;
            _client = client;
            _eventHandlerConfig = eventHandlerConfig;
        }

        public override async Task Call<TRequest>(TRequest request)
        {
            if (!(request is MessageData messageData))
            {
                throw new Exception("injected wrong implementation");
            }

            if (WebhookConfig.AuthenticationConfig.Type != AuthenticationType.None)
            {
                await AcquireTokenHandler.GetToken(_client);
            }

            //todo remove in v1
            var innerPayload = string.Empty;
            //var innerPayload = ModelParser.GetInnerPayload(messageData.Payload, _eventHandlerConfig.WebHookConfig.ModelToParse);
            var orderCode = ModelParser.ParsePayloadPropertyAsGuid("OrderCode", messageData.Payload);

            void TelemetryEvent(string msg)
            {
                BigBrother.Publish(new HttpClientFailure(messageData.Handle, messageData.Type, messageData.Payload, msg));
            }

            var response = await _client.ExecuteAsJsonReliably(WebhookConfig.HttpVerb, WebhookConfig.Uri, innerPayload, TelemetryEvent);

            BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, messageData.Payload, response.IsSuccessStatusCode.ToString()));

            //todo remove this such that the raw payload is what is sent back from the webhook to the callback
            var payload = new HttpResponseDto
            {
                OrderCode = orderCode,
                Content = await response.Content.ReadAsStringAsync(),
                StatusCode = (int)response.StatusCode
            };
            messageData.CallbackPayload = JsonConvert.SerializeObject(payload);

            BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, messageData.CallbackPayload));

            //call callback
            var eswHandler = _eventHandlerFactory.CreateWebhookHandler(_eventHandlerConfig.CallbackConfig.Name);

            await eswHandler.Call(messageData);
        }
    }
}
