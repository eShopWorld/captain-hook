using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using Polly;

namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// Http client extensions to make calls for different http verbs reliably
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Entry point for a generic http request which reports on the request and tries with exponential back-off for transient failure.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="httpVerb"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="telemetryRequest"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> ExecuteAsJsonReliably(
            this HttpClient client,
            HttpVerb httpVerb,
            string uri,
            string payload,
            Action<string> telemetryRequest,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            switch (httpVerb)
            {
                case HttpVerb.Get:
                    return await client.GetAsJsonReliably(uri, telemetryRequest, contentType, token);

                case HttpVerb.Put:
                    return await client.PutAsJsonReliably(uri, payload, telemetryRequest, contentType, token);

                case HttpVerb.Post:
                    return await client.PostAsJsonReliably(uri, payload, telemetryRequest, contentType, token);

                case HttpVerb.Patch:
                    return await client.PatchAsJsonReliably(uri, payload, telemetryRequest, contentType, token);

                default:
                    throw new ArgumentOutOfRangeException(nameof(httpVerb), httpVerb, "no valid http verb found");
            }
        }

        /// <summary>
        /// Post content to the endpoint
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="telemetryRequest"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostAsJsonReliably(
        this HttpClient client,
        string uri,
        string payload,
        Action<string> telemetryRequest,
        string contentType = "application/json",
        CancellationToken token = default)
        {
            var result = await RetryRequest(() => client.PostAsync(uri, new StringContent(payload, Encoding.UTF8, contentType), token), telemetryRequest);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="telemetryRequest"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PutAsJsonReliably(
            this HttpClient client,
            string uri,
            string payload,
            Action<string> telemetryRequest,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            var result = await RetryRequest(() => client.PutAsync(uri, new StringContent(payload, Encoding.UTF8, contentType), token), telemetryRequest);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="telemetryRequest"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PatchAsJsonReliably(
            this HttpClient client,
            string uri,
            string payload,
            Action<string> telemetryRequest,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            var result = await RetryRequest(() => client.PatchAsync(uri, new StringContent(payload, Encoding.UTF8, contentType), token), telemetryRequest);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="telemetryRequest"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> GetAsJsonReliably(
            this HttpClient client,
            string uri,
            Action<string> telemetryRequest,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            var result = await RetryRequest(() => client.GetAsync(uri, token), telemetryRequest);

            return result;
        }

        /// <summary>
        /// Executes the supplied func with reties and reports on it if something goes wrong ideally to BigBrother
        /// </summary>
        /// <param name="makeTheCall"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> RetryRequest(
            Func<Task<HttpResponseMessage>> makeTheCall,
            Action<string> report)
        {
            var response = await Policy.HandleResult<HttpResponseMessage>(
                    message =>
                        message.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        message.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(new[]
                {
                    //todo config this + jitter
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(30)

                }, (result, timeSpan, retryCount, context) =>
                {
                    report($"retry count {retryCount} of {context.Count}");
                }).ExecuteAsync(makeTheCall.Invoke);

            return response;
        }
    }
}
