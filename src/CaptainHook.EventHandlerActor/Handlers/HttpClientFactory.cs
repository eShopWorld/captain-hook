using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common.Configuration;
using JetBrains.Annotations;

namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// Builds out a http client for the given request flow
    /// </summary>
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly ConcurrentDictionary<string, HttpClient> _httpClients;

        public HttpClientFactory()
        {
            _httpClients = new ConcurrentDictionary<string, HttpClient>();
        }

        public HttpClientFactory(Dictionary<string, HttpClient> httpClients)
        {
            _httpClients = new ConcurrentDictionary<string, HttpClient>();

            foreach (var (key, value) in httpClients)
            {
                _httpClients.TryAdd(key, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public HttpClient Get(WebhookConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var uriString = config.Uri?.Replace("{", string.Empty).Replace("}", string.Empty);
            var uri = new Uri(uriString);

            if (_httpClients.TryGetValue(uri.Host.ToLowerInvariant(), out var httpClient))
            {
                return httpClient;
            }

            httpClient = new HttpClient {Timeout = config.Timeout};
            var result = _httpClients.TryAdd(uri.Host.ToLowerInvariant(), httpClient);

            if (!result)
            {
                throw new ArgumentNullException(nameof(httpClient), $"HttpClient for {uri.Host} could not be added to the dictionary of http clients");
            }
            return httpClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public HttpClient Get(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));

            var uri = new Uri(endpoint);
            if (_httpClients.TryGetValue(uri.Host.ToLowerInvariant(), out var httpClient))
            {
                return httpClient;
            }

            httpClient = new HttpClient();
            var result = _httpClients.TryAdd(uri.Host.ToLowerInvariant(), httpClient);

            if (!result)
            {
                throw new ArgumentNullException(nameof(httpClient), $"HttpClient for {uri} could not be added to the dictionary of http clients");
            }
            return httpClient;
        }
    }
}