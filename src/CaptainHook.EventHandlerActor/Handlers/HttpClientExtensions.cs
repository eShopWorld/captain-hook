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
        /// <param name="logger"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> ExecuteAsJsonReliably(
            this HttpClient client,
            HttpVerb httpVerb,
            Uri uri,
            string payload,
            HttpFailureLogger logger,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            switch (httpVerb)
            {
                case HttpVerb.Get:
                    return await client.GetAsJsonReliably(uri, logger, contentType, token);

                case HttpVerb.Put:
                    return await client.PutAsJsonReliably(uri, payload, logger, contentType, token);

                case HttpVerb.Post:
                    return await client.PostAsJsonReliably(uri, payload, logger, contentType, token);

                case HttpVerb.Patch:
                    return await client.PatchAsJsonReliably(uri, payload, logger, contentType, token);

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
        /// <param name="logger"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostAsJsonReliably(
        this HttpClient client,
        Uri uri,
        string payload,
        HttpFailureLogger logger,
        string contentType = "application/json",
        CancellationToken token = default)
        {
            var result = await RetryRequest(() => client.PostAsync(uri, new StringContent(payload, Encoding.UTF8, contentType), token), logger);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="logger"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PutAsJsonReliably(
            this HttpClient client,
            Uri uri,
            string payload,
            HttpFailureLogger logger,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            var result = await RetryRequest(() => client.PutAsync(uri, new StringContent(payload, Encoding.UTF8, contentType), token), logger);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="logger"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PatchAsJsonReliably(
            this HttpClient client,
            Uri uri,
            string payload,
            HttpFailureLogger logger,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            var result = await RetryRequest(() => client.PatchAsync(uri, new StringContent(payload, Encoding.UTF8, contentType), token), logger);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="logger"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> GetAsJsonReliably(
                this HttpClient client,
                Uri uri,
                HttpFailureLogger logger,
                string contentType = "application/json",
                CancellationToken token = default)
        {
            var result = await RetryRequest(() => client.GetAsync(uri, token), logger);

            return result;
        }

        /// <summary>
        /// Executes the supplied func with reties and reports on it if something goes wrong ideally to BigBrother
        /// </summary>
        /// <param name="makeTheCall"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> RetryRequest(
            Func<Task<HttpResponseMessage>> makeTheCall,
            HttpFailureLogger logger)
        {
            //todo the retry statuscodes need to be customisable from the webhook config api
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
                    logger.Publish($"retry count {retryCount} of {context.Count}", result.Result.StatusCode, context.CorrelationId.ToString());

                }).ExecuteAsync(makeTheCall.Invoke);

            return response;
        }
    }
}
