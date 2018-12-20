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

    public class GenericEventHandler : IHandler
    {
        private readonly HttpClient _client;
        protected readonly IBigBrother BigBrother;
        protected readonly WebHookConfig WebHookConfig;
        protected readonly IAuthHandler AuthHandler;

        public GenericEventHandler(
            IAuthHandler authHandler,
            IBigBrother bigBrother,
            HttpClient client,
            WebHookConfig webHookConfig)
        {
            _client = client;
            AuthHandler = authHandler;
            BigBrother = bigBrother;
            WebHookConfig = webHookConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual async Task Call<TRequest>(TRequest request)
        {
            if (!(request is MessageData data))
            {
                throw new Exception("injected wrong implementation");
            }

            //make a call to client identity provider
            if (WebHookConfig.RequiresAuth)
            {
                await AuthHandler.GetToken(_client);
            }

            //call the platform something like 
            //call checkout
            var uri = WebHookConfig.Uri;

            if (data.Type == "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent")
            {
                uri = $"https://checkout-api.ci.eshopworld.net/api/v2/webhook/PutOrderConfirmationResult/{data.CallbackPayload.OrderCode}";
            }

            if (data.Type == "checkout.domain.infrastructure.domainevents.platformordercreatedomainevent")
            {
                uri = $"https://checkout-api.ci.eshopworld.net/api/v2/PutCorePlatformOrderCreateResult/{data.CallbackPayload.OrderCode}";
            }

            var response = await _client.PostAsJsonReliability(uri, data, BigBrother);

            BigBrother.Publish(new WebhookEvent(data.Handle, data.Type, data.Payload, response.IsSuccessStatusCode.ToString()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        [Obsolete]
        public virtual async Task<HttpResponseDto> Call<TRequest, TResponse>(TRequest request)
        {
            if (!(request is MessageData data))
            {
                throw new Exception("injected wrong implementation");
            }

            //make a call to client identity provider
            if (WebHookConfig.RequiresAuth)
            {
                await AuthHandler.GetToken(_client);
            }
            
            var response = await _client.PostAsJsonReliability(WebHookConfig.Uri, data, BigBrother);

            BigBrother.Publish(new WebhookEvent(data.Handle, data.Type, data.Payload, response.IsSuccessStatusCode.ToString()));

            var dto = new HttpResponseDto
            {
                Content = await response.Content.ReadAsStringAsync(),
                StatusCode = (int)response.StatusCode
            };

            return dto;
        }
    }
}