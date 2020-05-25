using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Telemetry;
using CaptainHook.Common.Telemetry.Web;
using Eshopworld.Core;

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
            Uri uri,
            HttpMethod httpMethod,
            WebHookHeaders headers
        )
        {
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
                    messageData.CorrelationId));
            }
            else
            {
                // request failed
                _bigBrother.Publish(new FailedWebHookEvent(
                    httpClient.DefaultRequestHeaders.ToString(),
                    response.Headers.ToString(),
                    messageData.Payload ?? string.Empty,
                    await GetPayloadAsync(response),
                    messageData.EventHandlerActorId,
                    messageData.Type,
                    $"Response status code {response.StatusCode}",
                    uri.AbsoluteUri,
                    httpMethod,
                    response.StatusCode,
                    messageData.CorrelationId)
                {
                    AuthToken = response.StatusCode == System.Net.HttpStatusCode.Unauthorized ? headers?.RequestHeaders?[Constants.Headers.Authorization] : string.Empty
                });
            }
        }

        private static async Task<string> GetPayloadAsync(HttpResponseMessage response)
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
