using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Telemetry;
using Eshopworld.Core;
using Newtonsoft.Json;
using Polly;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Extension to http client to send requests via polly with some retries
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="payload"></param>
        /// <param name="messageData"></param>
        /// <param name="bigBrother"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostAsJsonReliability(
            this HttpClient client,
            string uri,
            string payload,
            MessageData messageData,
            IBigBrother bigBrother,
            string contentType = "application/json",
            CancellationToken token = default)
        {
            var response = await Policy.HandleResult<HttpResponseMessage>(
                    message =>
                    message.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    message.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5)

                }, (result, timeSpan, retryCount, context) =>
                {
                        bigBrother.Publish(new HttpClientFailure(
                            messageData.Handle, 
                            messageData.Type, 
                            messageData.Payload,
                            $"retry count {retryCount} of {context.Count}"));
                    
                }).ExecuteAsync(() => client.PostAsJson(uri, payload, contentType, token));

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
            StringContent stringContent;
            
            if (payload is string content)
            {
                stringContent = new StringContent(content, Encoding.UTF8, contentType);
            }
            else
            {
                stringContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, contentType);
            }
            
            return await client.PostAsync(uri, stringContent, token);
        }
    }
}
