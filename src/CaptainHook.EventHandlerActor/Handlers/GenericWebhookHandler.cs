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
        protected readonly IAcquireTokenHandler AcquireTokenHandler;

        public GenericWebhookHandler(
            IAcquireTokenHandler acquireTokenHandler,
            IBigBrother bigBrother,
            HttpClient client,
            WebhookConfig webhookConfig)
        {
            _client = client;
            AcquireTokenHandler = acquireTokenHandler;
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
                if (!(request is MessageData messageData))
                {
                    throw new Exception("injected wrong implementation");
                }

                //make a call to client identity provider
                if (WebhookConfig.RequiresAuth)
                {
                    await AcquireTokenHandler.GetToken(_client);
                }

                var uri = WebhookConfig.Uri;

                //todo remove in v1
                var orderCode = ModelParser.ParseOrderCode(messageData.Payload);

                var innerPayload = messageData.Payload;
                if (!string.IsNullOrWhiteSpace(WebhookConfig.ModelToParse))
                {
                    innerPayload = ModelParser.GetInnerPayload(messageData.Payload, WebhookConfig.ModelToParse);
                }

                if (!string.IsNullOrWhiteSpace(messageData.CallbackPayload))
                {
                    innerPayload = messageData.CallbackPayload;
                }

                //todo remove in v1
                switch (messageData.Type)
                {
                    case "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent":
                    case "checkout.domain.infrastructure.domainevents.platformordercreatedomainevent":
                        uri = $"{WebhookConfig.Uri}/{orderCode}"; //todo remove in v1
                        break;
                }

                var response = await _client.PostAsJsonReliability(uri, innerPayload, messageData, BigBrother);

                BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, messageData.Payload, response.IsSuccessStatusCode.ToString()));

            }
            catch (Exception e)
            {
                BigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }
    }
}
