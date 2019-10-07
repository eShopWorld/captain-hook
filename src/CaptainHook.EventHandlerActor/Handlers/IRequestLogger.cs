using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IRequestLogger
    {
        Task LogAsync(
            HttpClient httpClient,
            HttpResponseMessage response,
            MessageData messageData,
            Uri uri,
            HttpMethod httpMethod
        );
    }
}