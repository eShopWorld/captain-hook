using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Nasty;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class WebhookResponseHandler : GenericWebhookHandler
    {
        private readonly HttpClient _client;
        private readonly EventHandlerConfig _eventHandlerConfig;
        private readonly IEventHandlerFactory _eventHandlerFactory;

        public WebhookResponseHandler(
            IEventHandlerFactory eventHandlerFactory,
            IAcquireTokenHandler acquireTokenHandler,
            IBigBrother bigBrother,
            HttpClient client,
            EventHandlerConfig eventHandlerConfig)
            : base(acquireTokenHandler, bigBrother, client, eventHandlerConfig.WebHookConfig)
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

            if (WebhookConfig.AuthenticationConfig.Type != AuthenticationType.None)
            {
                await AcquireTokenHandler.GetToken(_client);
            }

            //todo remove in v1
            var innerPayload = ModelParser.GetInnerPayload(messageData.Payload, _eventHandlerConfig.WebHookConfig.ModelToParse);
            var orderCode = ModelParser.ParseOrderCode(messageData.Payload);

            void TelemetryEvent(string msg)
            {
                BigBrother.Publish(new HttpClientFailure(messageData.Handle, messageData.Type, messageData.Payload, msg));
            }

            var response = await _client.ExecuteAsJsonReliably(WebhookConfig.HttpVerb, WebhookConfig.Uri, innerPayload, TelemetryEvent);

            BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, messageData.Payload, response.IsSuccessStatusCode.ToString()));

            //todo remove this such that the raw payload is what is sent back from the webhook to the callback
            var payload = new HttpResponseDto
            {
                OrderCode = orderCode,
                Content = await response.Content.ReadAsStringAsync(),
                StatusCode = (int)response.StatusCode
            };
            messageData.OrderCode = orderCode;
            messageData.Payload = JsonConvert.SerializeObject(payload);

            BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, messageData.CallbackPayload));

            //call callback
            var eswHandler = _eventHandlerFactory.CreateHandler($"{_eventHandlerConfig.CallbackConfig.Name}");

            await eswHandler.Call(messageData);
        }
    }
}
