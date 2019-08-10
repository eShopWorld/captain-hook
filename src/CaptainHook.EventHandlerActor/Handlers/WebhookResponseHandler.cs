﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using CaptainHook.Common.Telemetry.Web;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class WebhookResponseHandler : GenericWebhookHandler
    {
        private readonly EventHandlerConfig _eventHandlerConfig;
        private readonly IEventHandlerFactory _eventHandlerFactory;

        public WebhookResponseHandler(
            IEventHandlerFactory eventHandlerFactory,
            IAuthenticationHandlerFactory authenticationHandlerFactory,
            IRequestBuilder requestBuilder,
            IBigBrother bigBrother,
            IIndex<string, HttpClient> httpClients,
            EventHandlerConfig eventHandlerConfig)
            : base(authenticationHandlerFactory, requestBuilder, bigBrother, httpClients, eventHandlerConfig.WebhookConfig)
        {
            _eventHandlerFactory = eventHandlerFactory;
            _eventHandlerConfig = eventHandlerConfig;
        }

        public override async Task CallAsync<TRequest>(TRequest request, IDictionary<string, object> metadata, CancellationToken cancellationToken)
        {
            if (!(request is MessageData messageData))
            {
                throw new Exception("injected wrong implementation");
            }

            var uri = RequestBuilder.BuildUri(WebhookConfig, messageData.Payload);
            var httpVerb = RequestBuilder.SelectHttpVerb(WebhookConfig, messageData.Payload);
            var payload = RequestBuilder.BuildPayload(WebhookConfig, messageData.Payload, metadata);
            var authenticationScheme = RequestBuilder.SelectAuthenticationScheme(WebhookConfig, messageData.Payload);
            var config = RequestBuilder.SelectWebhookConfig(WebhookConfig, messageData.Payload);

            var httpClient = await GetHttpClient(cancellationToken, config, authenticationScheme, messageData.CorrelationId);

            //todo don't like this but it will do for now to keep the extensions clean from message implementation
            var handler = new HttpFailureLogger(BigBrother, messageData, uri.AbsoluteUri, httpVerb);

            var response = await httpClient.ExecuteAsJsonReliably(httpVerb, uri, payload, handler, "application/json", cancellationToken);

            BigBrother.Publish(
                new WebhookEvent(
                    messageData.Handle, 
                    messageData.Type, 
                    $"Response status code {response.StatusCode}", 
                    uri.AbsoluteUri, 
                    httpVerb, 
                    response.StatusCode,
                    messageData.CorrelationId
                ));

            if (metadata == null)
            {
                metadata = new Dictionary<string, object>();
            }
            else
            {
                metadata.Clear();
            }

            var content = await response.Content.ReadAsStringAsync();
            metadata.Add("HttpStatusCode", (int)response.StatusCode);
            metadata.Add("HttpResponseContent", content);

            //call callback
            var eswHandler = _eventHandlerFactory.CreateWebhookHandler(_eventHandlerConfig.CallbackConfig.Name);

            await eswHandler.CallAsync(messageData, metadata, cancellationToken);
        }
    }
}
