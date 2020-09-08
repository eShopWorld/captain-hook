using System;
using System.Net.Http;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Gets a http client for a particular uri. If one is not found it creates one, stores and reuses later
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        HttpClient Get(Uri uri);
    }
}