using System;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common;
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
        Uri BuildUri(WebhookConfig config, string payload);

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
        HttpMethod SelectHttpMethod(WebhookConfig webhookConfig, string payload);

        /// <summary>
        /// Determines the authentication scheme to use in the request.
        /// </summary>
        /// <param name="webhookConfig"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        WebhookConfig GetAuthenticationConfig(WebhookConfig webhookConfig, string payload);

        /// <summary>
        /// Selects the webhook config to use for the endpoint for which the request is destined based on supplied webhook rules and configs
        /// </summary>
        /// <param name="webhookConfig"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        WebhookConfig SelectWebhookConfig(WebhookConfig webhookConfig, string payload);

        /// <summary>
        /// Creates a dictionary of requests headers which are needed per request
        /// </summary>
        /// <param name="messageData">message wrapper</param>
        /// <param name="webhookConfig">config to use</param>
        /// <returns>list of headers</returns>
        WebHookHeaders GetHttpHeaders(WebhookConfig webhookConfig, MessageData messageData);
    }
}
