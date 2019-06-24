using System;
using System.Collections.Concurrent;
using System.Net.Http;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Common
{
    public interface IExtendedHttpClientFactory : IHttpClientFactory
    {
        HttpClient CreateClient(Uri uri, WebhookConfig config);
    }

    public class HttpClientFactory : IExtendedHttpClientFactory
    {
        private readonly ConcurrentDictionary<string, HttpClient> _clients;
        public HttpClientFactory()
        {
            _clients = new ConcurrentDictionary<string, HttpClient>();
        }

        public HttpClient CreateClient(string name)
        {
            if (_clients.TryGetValue(name.ToLower(), out var client))
            {
                return client;
            }

            var httpClient = new HttpClient();
            _clients.TryAdd(name, httpClient);

            return httpClient;
        }

        public HttpClient CreateClient(Uri uri, WebhookConfig config)
        {
            if (_clients.TryGetValue(uri.Host.ToLower(), out var client))
            {
                return client;
            }

            var httpClient = new HttpClient
            {
                Timeout = config.Timeout
            };
            _clients.TryAdd(uri.Host, httpClient);

            return httpClient;
        }
    }
}
