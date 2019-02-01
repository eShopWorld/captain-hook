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
                if (WebhookConfig.AuthenticationConfig.Type != AuthenticationType.None)
                {
                    await AcquireTokenHandler.GetToken(_client);
                }

                var uri = WebhookConfig.Uri;

                //todo remove to integration layer by v1
                switch (messageData.Type)
                {
                    case "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent":
                    case "checkout.domain.infrastructure.domainevents.platformordercreatedomainevent":
                        var orderCode = ModelParser.ParseOrderCode(messageData.Payload);
                        uri = $"{WebhookConfig.Uri}/{orderCode}"; //todo remove to integration layer by v1
                        break;
                }

                //todo refactor out
                var innerPayload = messageData.Payload;
                if (!string.IsNullOrWhiteSpace(WebhookConfig.ModelToParse))
                {
                    innerPayload = ModelParser.GetInnerPayload(messageData.Payload, WebhookConfig.ModelToParse);
                }

                //todo refactor out
                if (!string.IsNullOrWhiteSpace(messageData.CallbackPayload))
                {
                    innerPayload = messageData.CallbackPayload;
                }

                void TelemetryEvent(string msg)
                {
                    BigBrother.Publish(new HttpClientFailure(messageData.Handle, messageData.Type, messageData.Payload, msg));
                }
                
                var response = await _client.ExecuteAsJsonReliably(WebhookConfig.Verb, uri, innerPayload, TelemetryEvent);

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
