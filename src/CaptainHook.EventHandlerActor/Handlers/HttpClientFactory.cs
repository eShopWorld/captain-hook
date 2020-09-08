using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;

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

        public HttpClient Get(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (_httpClients.TryGetValue(uri.Host.ToLowerInvariant(), out var httpClient))
            {
                return httpClient;
            }

            httpClient = new HttpClient();

            var result = _httpClients.TryAdd(uri.Host.ToLowerInvariant(), httpClient);

            if (!result)
            {
                throw new ArgumentNullException(nameof(httpClient), $"HttpClient for {uri.Host} could not be added to the dictionary of http clients");
            }
            return httpClient;
        }
    }
}