namespace CaptainHook.EventHandlerActor.Handlers
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Authentication;
    using Common;
    using Common.Nasty;
    using Common.Telemetry;
    using Eshopworld.Core;
    using Newtonsoft.Json;

    public class RetailerEventHandler : GenericEventHandler
    {
        private readonly HttpClient _client;
        private readonly IHandlerFactory _handlerFactory;

        public RetailerEventHandler(
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

            //todo get publishers to send clean models
            var domainEventConfig = WebHookConfig.DomainEvents.FirstOrDefault(t => t.Name == data.Type);

            if (domainEventConfig != null)
            {
                data.Payload = ModelParser.GetInnerPayload(data.Payload, domainEventConfig.Path);
            }

            //go out to the retailer on their api, in the case of goc it's a esw endpoint
            var uri = new Uri(WebHookConfig.Uri);
            var response = await _client.PostAsJsonReliability(uri.AbsoluteUri, data, BigBrother);

            BigBrother.Publish(new WebhookEvent(data.Handle, data.Type, data.Payload, response.IsSuccessStatusCode.ToString()));

            //call callback
            
            var eswHandler = _handlerFactory.CreateHandler("esw", "esw");

            var payload = new HttpResponseDto
            {
                Content = await response.Content.ReadAsStringAsync(),
                StatusCode = (int)response.StatusCode
            };

            data.Payload = JsonConvert.SerializeObject(payload);
            await eswHandler.Call(data);
        }
    }
}