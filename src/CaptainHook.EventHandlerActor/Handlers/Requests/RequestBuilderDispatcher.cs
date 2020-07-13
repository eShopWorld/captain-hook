using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Autofac.Features.Indexed;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;

namespace CaptainHook.EventHandlerActor.Handlers.Requests
{
    public class RequestBuilderDispatcher: IRequestBuilder
    {
        private const RuleAction DefaultRoute = RuleAction.Route;

        private readonly IIndex<RuleAction, IRequestBuilder> _requestBuilderIndex;

        public RequestBuilderDispatcher(IIndex<RuleAction, IRequestBuilder> requestBuilderIndex)
        {
            _requestBuilderIndex = requestBuilderIndex;
        }

        private IRequestBuilder PickRequestBuilder(WebhookConfig config)
        {
            var hasRouteAndReplace = config.WebhookRequestRules.Any(r => r.Destination.RuleAction == RuleAction.RouteAndReplace);
            return hasRouteAndReplace
                ? _requestBuilderIndex[RuleAction.RouteAndReplace]
                : _requestBuilderIndex[DefaultRoute];
        }

        public Uri BuildUri(WebhookConfig config, string payload) => PickRequestBuilder(config).BuildUri(config, payload);

        public string BuildPayload(WebhookConfig config, string sourcePayload, IDictionary<string, object> data = null) 
            => PickRequestBuilder(config).BuildPayload(config, sourcePayload, data);

        public HttpMethod SelectHttpMethod(WebhookConfig webhookConfig, string payload) =>
            PickRequestBuilder(webhookConfig).SelectHttpMethod(webhookConfig, payload);

        public WebhookConfig GetAuthenticationConfig(WebhookConfig webhookConfig, string payload) =>
            PickRequestBuilder(webhookConfig).GetAuthenticationConfig(webhookConfig, payload);

        public WebhookConfig SelectWebhookConfig(WebhookConfig webhookConfig, string payload) =>
            PickRequestBuilder(webhookConfig).SelectWebhookConfig(webhookConfig, payload);

        public WebHookHeaders GetHttpHeaders(WebhookConfig webhookConfig, MessageData messageData) =>
            PickRequestBuilder(webhookConfig).GetHttpHeaders(webhookConfig, messageData);
    }
}