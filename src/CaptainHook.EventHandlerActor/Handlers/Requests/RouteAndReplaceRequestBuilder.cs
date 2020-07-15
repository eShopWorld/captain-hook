using System;
using System.Linq;
using CaptainHook.Common.Configuration;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers.Requests
{
    public class RouteAndReplaceRequestBuilder: DefaultRequestBuilder
    {
        public RouteAndReplaceRequestBuilder(IBigBrother bigBrother): base(bigBrother)
        {
        }

        protected override Uri BuildUriFromExistingConfig(WebhookConfig config, string payload)
        {
            var routeAndReplaceRule = config.WebhookRequestRules.FirstOrDefault(r => r.Destination.RuleAction == RuleAction.RouteAndReplace);
            if (routeAndReplaceRule != null)
            {
                return null;
            }

            return null;
        }
    }
}