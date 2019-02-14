using System.Collections.Generic;
using CaptainHook.Common.Configuration;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IRequestBuilder
    {
        string BuildUri(WebhookConfig config, string payload);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="sourcePayload"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        string BuildPayload(WebhookConfig config, string sourcePayload, Dictionary<string, string> data);
    }
}