﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Nasty;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class WebhookResponseHandler : GenericWebhookHandler
    {
        private readonly HttpClient _client;
        private readonly EventHandlerConfig _eventHandlerConfig;
        private readonly IEventHandlerFactory _eventHandlerFactory;

        public WebhookResponseHandler(
            IEventHandlerFactory eventHandlerFactory,
            IAuthHandler authHandler,
            IBigBrother bigBrother,
            HttpClient client,
            EventHandlerConfig eventHandlerConfig)
            : base(authHandler, bigBrother, client, eventHandlerConfig.WebHookConfig)
        {
            _eventHandlerFactory = eventHandlerFactory;
            _client = client;
            _eventHandlerConfig = eventHandlerConfig;
        }

        public override async Task Call<TRequest>(TRequest request)
        {
            if (!(request is MessageData messageData))
            {
                throw new Exception("injected wrong implementation");
            }

            if (WebhookConfig.RequiresAuth)
            {
                await AuthHandler.GetToken(_client);
            }

            //todo remove in v1
            var innerPayload = ModelParser.GetInnerPayload(messageData.Payload, _eventHandlerConfig.WebHookConfig.ModelToParse);
            var orderCode = ModelParser.ParseOrderCode(messageData.Payload);

            var response = await _client.PostAsJsonReliability(WebhookConfig.Uri, innerPayload, messageData, BigBrother);

            BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, messageData.Payload, response.IsSuccessStatusCode.ToString()));

            //call callback
            var eswHandler = _eventHandlerFactory.CreateHandler($"{_eventHandlerConfig.CallbackConfig.Name}-callback");

            var payload = new HttpResponseDto
            {
                OrderCode = orderCode,
                Content = await response.Content.ReadAsStringAsync(),
                StatusCode = (int)response.StatusCode   
            };

            messageData.OrderCode = orderCode;
            messageData.CallbackPayload = payload;
            await eswHandler.Call(messageData);
        }
    }
}
