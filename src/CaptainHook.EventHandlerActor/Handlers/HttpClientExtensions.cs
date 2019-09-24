﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
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
        /// <param name="httpMethod"></param>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <param name="payload"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> SendRequestReliablyAsync(
            this HttpClient client,
            HttpMethod httpMethod,
            Uri uri,
            Dictionary<string, string> headers,
            string payload,
            CancellationToken token = default)
        {
            var request = new HttpRequestMessage(httpMethod, uri)
            {
                Content = new StringContent(payload, Encoding.UTF8, headers[Constants.Headers.ContentType])
            };

            foreach (var key in headers.Keys)
            {
                if (request.Headers.Contains(key))
                {
                    request.Headers.Remove(key);
                }

                request.Headers.Add(key, headers[key]);
            }

            var result = await RetryRequest(() => client.SendAsync(request, token));

            return result;
        }

        /// <summary>
        /// Executes the supplied func with reties and reports on it if something goes wrong ideally to BigBrother
        /// </summary>
        /// <param name="makeTheCall"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> RetryRequest(
            Func<Task<HttpResponseMessage>> makeTheCall)
        {
            //todo the retry status codes need to be customisable from the webhook config api
            var response = await Policy.HandleResult<HttpResponseMessage>(
                    message =>
                        message.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        message.StatusCode == HttpStatusCode.TooManyRequests)

                .WaitAndRetryAsync(new[]
                {
                    //todo config this + jitter
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(30)

                }).ExecuteAsync(makeTheCall.Invoke);

            return response;
        }
    }
}
