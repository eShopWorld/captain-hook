namespace CaptainHook.EventHandlerActor.Handlers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Telemetry;
    using Eshopworld.Core;
    using Newtonsoft.Json;
    using Polly;

    public static class HttpClientExtensions
    {
        /// <summary>
        /// Extenion to http client to send requests via polly with some retries
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="bigBrother"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostAsJsonReliability<T>(
            this HttpClient client,
            string uri,
            T payload,
            IBigBrother bigBrother,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            var response = await Policy
                .HandleResult<HttpResponseMessage>(message => message.StatusCode == HttpStatusCode.ServiceUnavailable || message.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),TimeSpan.FromSeconds(2),TimeSpan.FromSeconds(5)

                }, (result, timeSpan, retryCount, context) =>
                {
                    //todo fill this in correctly for telemetry
                    bigBrother.Publish(new HttpClientFailure(Guid.NewGuid(), string.Empty, string.Empty, $"retry count {retryCount} of {context.Count}"));
                })
                .ExecuteAsync(() => client.PostAsJson(uri, payload, contentType, token));

            return response;
        }

        /// <summary>
        /// Extension to Http client to send generic type to destination as JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostAsJson<T>(
            this HttpClient client,
            string uri,
            T payload,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF32, contentType);
            return await client.PostAsync(uri, content, token);
        }
    }
}