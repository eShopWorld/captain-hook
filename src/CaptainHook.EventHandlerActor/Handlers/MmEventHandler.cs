namespace CaptainHook.EventHandlerActor.Handlers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Authentication;
    using Common;
    using Common.Nasty;
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
                //todo handler failure here ie call the web hook with the message
            }

            //todo move order code to body so we don't have to deal with it in CH
            var orderCode = ModelParser.ParseOrderCode(data.Payload);

            //todo polly
            var mmResponse = await _client.PostAsJsonAsync($"{WebHookConfig.Uri}/{orderCode}", data.Payload);

            if (!mmResponse.IsSuccessStatusCode)
            {
                //todo failure here that is not a retry
            }

            var domainType = ModelParser.ParseDomainType(data.Payload);
            var eswHandler = _handlerFactory.CreateHandler("esw");

            var payload = new HttpResponseDto
            {
                Content = await mmResponse.Content.ReadAsStringAsync(),
                StatusCode = (int) mmResponse.StatusCode
            };

            await eswHandler.Call(payload);
        }
    }
}