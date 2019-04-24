﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class EventHandlerFactory : IEventHandlerFactory
    {
        private readonly IIndex<string, HttpClient> _httpClients;
        private readonly IBigBrother _bigBrother;
        private readonly IIndex<string, EventHandlerConfig> _eventHandlerConfig;
        private readonly IIndex<string, WebhookConfig> _webHookConfig;
        private readonly IAuthHandlerFactory _authHandlerFactory;

        public EventHandlerFactory(
            IIndex<string, HttpClient> httpClients,
            IBigBrother bigBrother,
            IIndex<string, EventHandlerConfig> eventHandlerConfig,
            IIndex<string, WebhookConfig> webHookConfig,
            IAuthHandlerFactory authHandlerFactory)
        {
            _httpClients = httpClients;
            _bigBrother = bigBrother;
            _eventHandlerConfig = eventHandlerConfig;
            _authHandlerFactory = authHandlerFactory;
            _webHookConfig = webHookConfig;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create the custom handler such that we get a mapping from the webhook to the handler selected
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IHandler> CreateEventHandlerAsync(string eventType, CancellationToken cancellationToken)
        {
            if (!_eventHandlerConfig.TryGetValue(eventType.ToLower(), out var eventHandlerConfig))
            {
                throw new Exception($"Boom, handler event type {eventType} was not found, cannot process the message");
            }

            var authHandler = await _authHandlerFactory.GetAsync($"{eventType}-webhook", cancellationToken);
            if (eventHandlerConfig.CallBackEnabled)
            {
                return new WebhookResponseHandler(
                    this,
                    authHandler,
                    new RequestBuilder(),
                    _bigBrother,
                    _httpClients[eventHandlerConfig.WebHookConfig.Name.ToLower()],
                    eventHandlerConfig);
            }

            return new GenericWebhookHandler(
                authHandler,
                new RequestBuilder(),
                _bigBrother,
                _httpClients[eventHandlerConfig.WebHookConfig.Name.ToLower()],
                eventHandlerConfig.WebHookConfig);
        }

        /// <summary>
        /// Creates a single fire and forget webhook handler
        /// Need this here for now to select the handler for the callback
        /// </summary>
        /// <param name="webHookName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IHandler> CreateWebhookHandlerAsync(string webHookName, CancellationToken cancellationToken)
        {
            if (!_webHookConfig.TryGetValue(webHookName.ToLower(), out var webhookConfig))
            {
                throw new Exception("Boom, handler webhook not found cannot process the message");
            }

            var authHandler = await _authHandlerFactory.GetAsync(webHookName, cancellationToken);

            return new GenericWebhookHandler(
                authHandler,
                new RequestBuilder(),
                _bigBrother,
                _httpClients[webHookName.ToLower()],
                webhookConfig);
        }
    }
}
