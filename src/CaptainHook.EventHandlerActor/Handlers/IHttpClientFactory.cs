using System.Net.Http;
using CaptainHook.Common.Configuration;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Gets a httpclient which was created for a particular endpoint at startup. If one does not exist an ArgumentNullException is thrown
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        HttpClient Get(WebhookConfig config);

        /// <summary>
        /// Gets a http client for a particular uri. If one is not found it creates one, stores and reuses later
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        HttpClient Get(string endpoint);
    }
}