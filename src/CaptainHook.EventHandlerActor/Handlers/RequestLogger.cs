using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using CaptainHook.Common.Telemetry.Web;
using Eshopworld.Core;
using Eshopworld.DevOps;
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

        public RequestLogger(IBigBrother bigBrother)
        {
            _bigBrother = bigBrother;
        }

        public async Task LogAsync(
            HttpClient httpClient,
            HttpResponseMessage response,
            MessageData messageData,
            string actualPayload,
            Uri uri,
            HttpMethod httpMethod,
            WebHookHeaders headers,
            WebhookConfig config
        )
        {
            var webhookRules = CollectRules (config);
            var canLogPayload = (EswDevOpsSdk.GetEnvironment() != DeploymentEnvironment.Prod && EswDevOpsSdk.GetEnvironment() != DeploymentEnvironment.Sand);

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
                // request failed
                _bigBrother.Publish(new FailedWebHookEvent(
                    httpClient.DefaultRequestHeaders.ToString(),
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

        private string CollectRules (WebhookConfig config)
        {
            var list = config?.WebhookRequestRules?.Select (rule => (src: rule.Source, dest: rule.Destination)).ToArray ();
            return list == null ? null: JsonConvert.SerializeObject (list, Formatting.None);
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
