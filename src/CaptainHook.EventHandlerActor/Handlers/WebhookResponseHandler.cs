using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Nasty;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

            if (WebhookConfig.RequiresAuth)
            {
                await AcquireTokenHandler.GetToken(_client);
            }

            //todo remove in v1
            var innerPayload = ModelParser.GetInnerPayload(messageData.Payload, _eventHandlerConfig.WebHookConfig.ModelToParse);
            var orderCode = ModelParser.ParseOrderCode(messageData.Payload);

            var response = await _client.PostAsJsonReliability(WebhookConfig.Uri, innerPayload, messageData, BigBrother);

            BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, messageData.Payload, response.IsSuccessStatusCode.ToString()));

            //call callback
            var eswHandler = _eventHandlerFactory.CreateHandler($"{_eventHandlerConfig.CallbackConfig.Name}");

            var payload = new HttpResponseDto
            {
                OrderCode = orderCode,
                Content = await response.Content.ReadAsStringAsync(),
                StatusCode = (int)response.StatusCode
            };

            messageData.OrderCode = orderCode;
            messageData.CallbackPayload = JsonConvert.SerializeObject(payload);

            BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, messageData.CallbackPayload));

            await eswHandler.Call(messageData);
        }
    }
}
