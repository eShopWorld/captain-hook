using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            string httpVerb,
            string uri,
            string payload,
            Action<string> telemetryRequest,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(httpVerb))
            {
                throw new ArgumentException("is null empty or whitespace in your webhook or callback configuration", nameof(httpVerb));
            }

            if (string.Equals("get", httpVerb, StringComparison.OrdinalIgnoreCase))
            {
                return await client.GetAsJsonReliably(uri, telemetryRequest, contentType, token);
            }

            if (string.Equals("post", httpVerb, StringComparison.OrdinalIgnoreCase))
            {
                return await client.PostAsJsonReliably(uri, payload, telemetryRequest, contentType, token);
            }

            if (string.Equals("put", httpVerb, StringComparison.OrdinalIgnoreCase))
            {
                return await client.PutAsJsonReliably(uri, payload, telemetryRequest, contentType, token);
            }

            if (string.Equals("patch", httpVerb, StringComparison.OrdinalIgnoreCase))
            {
                return await client.PatchAsJsonReliably(uri, payload, telemetryRequest, contentType, token);
            }

            throw new Exception($"no valid http verb found for {httpVerb}");
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
