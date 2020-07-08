using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IRequestLogger
    {
        Task LogAsync(
            HttpClient httpClient,
            HttpResponseMessage response,
            MessageData messageData,
            string actualPayload,
            Uri uri,
            HttpMethod httpMethod,
            WebHookHeaders headers,
            WebhookConfig config
        );
    }
}
