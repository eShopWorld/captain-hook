using System.Collections.Concurrent;
using System.Net.Http;

namespace CaptainHook.Common
{
    public class HttpClientFactory : IHttpClientFactory
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
    }
}