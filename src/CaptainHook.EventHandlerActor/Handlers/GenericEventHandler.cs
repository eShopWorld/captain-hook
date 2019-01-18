using CaptainHook.Common.Nasty;

namespace CaptainHook.EventHandlerActor.Handlers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Authentication;
    using Common;
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
            try
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

                var orderCode = ModelParser.ParseOrderCode(data.Payload);
                //todo move this out of here into the config management
                if (data.Type == "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent")
                {
                    uri = $"https://checkout-api.ci.eshopworld.net/api/v2/webhook/PutOrderConfirmationResult/{orderCode}";
                }

                if (data.Type == "checkout.domain.infrastructure.domainevents.platformordercreatedomainevent")
                {
                    uri = $"https://checkout-api.ci.eshopworld.net/api/v2/PutCorePlatformOrderCreateResult/{orderCode}";
                }

                var response = await _client.PostAsJsonReliability(uri, data, BigBrother);

                BigBrother.Publish(new WebhookEvent(data.Handle, data.Type, data.Payload, response.IsSuccessStatusCode.ToString()));

            }
            catch (Exception e)
            {
                BigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }
    }
}
