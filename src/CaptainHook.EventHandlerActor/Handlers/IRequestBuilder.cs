using System.Collections.Generic;
using CaptainHook.Common.Configuration;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IRequestBuilder
    {
        /// <summary>
        /// Constructs a URI based on the set of webhook rules as well as the injected webhook configurations
        /// </summary>
        /// <param name="config"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        string BuildUri(WebhookConfig config, string payload);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="sourcePayload"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        string BuildPayload(WebhookConfig config, string sourcePayload, IDictionary<string, string> data = null);
    }
}
