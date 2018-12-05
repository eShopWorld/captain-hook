namespace CaptainHook.EventHandlerActor.Handlers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Authentication;
    using Common;
    using Common.Nasty;
    using Common.Telemetry;
    using Eshopworld.Core;

    public class MmEventHandler : GenericEventHandler
    {
        private readonly HttpClient _client;
        private readonly IHandlerFactory _handlerFactory;

        public MmEventHandler(
            IHandlerFactory handlerFactory,
            HttpClient client,
            IBigBrother bigBrother,
            WebHookConfig webHookConfig, 
            IAuthHandler authHandler)
            : base(authHandler, bigBrother, client, webHookConfig)
        {
            _handlerFactory = handlerFactory;
            _client = client;
        }

        public override async Task Call<TRequest>(TRequest request)
        {
            if (!(request is MessageData data))
            {
                throw new Exception("injected wrong implementation");
            }

            if (WebHookConfig.RequiresAuth)
            {
                await AuthHandler.GetToken(_client);
            }

            //todo move order code to body so we don't have to deal with it in CH
            var orderCode = ModelParser.ParseOrderCode(data.Payload);

            var response = await _client.PostAsJsonReliability($"{WebHookConfig.Uri}/{orderCode}", data.Payload, BigBrother);

            BigBrother.Publish(new WebhookEvent(data.Handle, data.Type, data.Payload, response.IsSuccessStatusCode.ToString()));

            var eswHandler = _handlerFactory.CreateHandler("esw");

            var payload = new HttpResponseDto
            {
                Content = await response.Content.ReadAsStringAsync(),
                StatusCode = (int) response.StatusCode
            };

            await eswHandler.Call(payload);
        }
    }
}