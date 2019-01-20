using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Nasty;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// Generic WebHookConfig Handler which executes the call to a webhook based on the supplied configuration
    /// </summary>
    public class GenericWebhookHandler : IHandler
    {
        private readonly HttpClient _client;
        protected readonly IBigBrother BigBrother;
        protected readonly WebhookConfig WebhookConfig;
        protected readonly IAuthHandler AuthHandler;

        public GenericWebhookHandler(
            IAuthHandler authHandler,
            IBigBrother bigBrother,
            HttpClient client,
            WebhookConfig webhookConfig)
        {
            _client = client;
            AuthHandler = authHandler;
            BigBrother = bigBrother;
            WebhookConfig = webhookConfig;
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
                if (WebhookConfig.RequiresAuth)
                {
                    await AuthHandler.GetToken(_client);
                }
                
                var uri = WebhookConfig.Uri;

                //todo move this out of here into a jpath executor so that the guid is added via a handler based on config
                var orderCode = ModelParser.ParseOrderCode(data.Payload);
                switch (data.Type)
                {
                    case "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent":
                    case "checkout.domain.infrastructure.domainevents.platformordercreatedomainevent":
                        uri = $"{WebhookConfig.Uri}/{orderCode}";
                        break;
                }

                var response = await _client.PostAsJsonReliability(uri, data.Payload, data, BigBrother);

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
