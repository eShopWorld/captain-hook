namespace CaptainHook.EventHandlerActor.Handlers
{
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public static class HttpClientExtensions
    {
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
        public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(
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