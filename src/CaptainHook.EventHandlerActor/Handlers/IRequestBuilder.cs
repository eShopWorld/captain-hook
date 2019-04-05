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
        /// Builds the payload for the http request based on supplied configurations
        /// </summary>
        /// <param name="config"></param>
        /// <param name="sourcePayload"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        string BuildPayload(WebhookConfig config, string sourcePayload, IDictionary<string, object> data = null);

        /// <summary>
        /// Determines the http verb to use in the request
        /// </summary>
        /// <param name="webhookConfig"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        HttpVerb SelectHttpVerb(WebhookConfig webhookConfig, string payload);
    }
}
