using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.FeatureFlags;
using CaptainHook.Common.Telemetry;
using CaptainHook.Common.Telemetry.Web;
using CaptainHook.EventHandlerActor.Utils;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// Handles logging successful or failed webhook calls to the destination endpoints
    /// Extends the telemetry but emitting all request and response properties in the failure flows
    /// </summary>
    public class RequestLogger : IRequestLogger
    {
        private readonly IBigBrother _bigBrother;
        private readonly FeatureFlagsConfiguration _featureFlags;

        public RequestLogger(
            IBigBrother bigBrother,
            FeatureFlagsConfiguration featureFlags)
        {
            _bigBrother = bigBrother;
            _featureFlags = featureFlags;
        }

        public async Task LogAsync(
            HttpResponseMessage response,
            MessageData messageData,
            string actualPayload,
            Uri uri,
            HttpMethod httpMethod,
            WebHookHeaders headers,
            WebhookConfig config
        )
        {
            var webhookRules = CollectRules(config);

            if (response.IsSuccessStatusCode)
            {
                // request was successful
                _bigBrother.Publish(new WebhookEvent(
                    messageData.EventHandlerActorId,
                    messageData.Type,
                    $"Response status code {response.StatusCode}",
                    uri.AbsoluteUri,
                    httpMethod,
                    response.StatusCode,
                    messageData.CorrelationId,
                    webhookRules));
            }
            else
            {
                var canLogPayload = !(_featureFlags.GetFlag<DisablePayloadLoggingFeatureFlag>()?.IsEnabled ?? false);

                // request failed
                _bigBrother.Publish(new FailedWebhookEvent(
                    headers.RequestHeaders.ToDebugString(),
                    response.Headers.ToString(),
                    canLogPayload ? messageData.Payload ?? string.Empty : string.Empty, // request body
                    await GetResponsePayloadAsync(response), // response body
                    canLogPayload ? actualPayload : string.Empty,  // messagePayload
                    messageData.EventHandlerActorId,
                    messageData.Type,
                    $"Response status code {response.StatusCode}",
                    uri.AbsoluteUri,
                    httpMethod,
                    response.StatusCode,
                    messageData.CorrelationId,
                    webhookRules)
                {
                    AuthToken = response.StatusCode == System.Net.HttpStatusCode.Unauthorized ? headers?.RequestHeaders?[Constants.Headers.Authorization] : string.Empty
                });
            }
        }

        private string CollectRules(WebhookConfig config)
        {
            var list = config?.WebhookRequestRules?.Select(rule => (src: rule.Source, dest: rule.Destination)).ToArray();
            return list == null ? null : JsonConvert.SerializeObject(list, Formatting.None);
        }

        private static async Task<string> GetResponsePayloadAsync(HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return string.Empty;
            }
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
