using System;
using System.Linq;
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
        private readonly IHandlerFactory _handlerFactory;

        public WebhookResponseHandler(
            IHandlerFactory handlerFactory,
            IAuthHandler authHandler,
            IBigBrother bigBrother,
            HttpClient client,
            EventHandlerConfig eventHandlerConfig)
            : base(authHandler, bigBrother, client, eventHandlerConfig.WebHookConfig)
        {
            _handlerFactory = handlerFactory;
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
            
            var innerPayload = ModelParser.GetInnerPayload(messageData.Payload, _eventHandlerConfig.EventParsers.ModelQueryPath);
            var orderCode = ModelParser.ParseOrderCode(messageData.Payload);

            var response = await _client.PostAsJsonReliability(WebhookConfig.Uri, innerPayload, messageData, BigBrother);

            BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, messageData.Payload, response.IsSuccessStatusCode.ToString()));

            //call callback
            var eswHandler = _handlerFactory.CreateHandler(_eventHandlerConfig.CallbackConfig.Name);

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
