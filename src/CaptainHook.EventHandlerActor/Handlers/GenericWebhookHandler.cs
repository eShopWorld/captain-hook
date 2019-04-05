using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
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
        protected readonly IRequestBuilder RequestBuilder;
        protected readonly WebhookConfig WebhookConfig;
        protected readonly IAcquireTokenHandler AcquireTokenHandler;

        public GenericWebhookHandler(
            IAcquireTokenHandler acquireTokenHandler,
            IRequestBuilder requestBuilder,
            IBigBrother bigBrother,
            HttpClient client,
            WebhookConfig webhookConfig)
        {
            _client = client;
            AcquireTokenHandler = acquireTokenHandler;
            BigBrother = bigBrother;
            RequestBuilder = requestBuilder;
            WebhookConfig = webhookConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public virtual async Task Call<TRequest>(TRequest request, IDictionary<string, object> metadata = null)
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

                var uri = RequestBuilder.BuildUri(WebhookConfig, messageData.Payload);
                var httpVerb = RequestBuilder.SelectHttpVerb(WebhookConfig, messageData.Payload);
                var payload = this.RequestBuilder.BuildPayload(this.WebhookConfig, messageData.Payload, metadata);

                void TelemetryEvent(string msg)
                {
                    BigBrother.Publish(new HttpClientFailure(messageData.Handle, messageData.Type, payload, msg));
                }

                var response = await _client.ExecuteAsJsonReliably(httpVerb, uri, payload, TelemetryEvent);
                
                BigBrother.Publish(new WebhookEvent(messageData.Handle, messageData.Type, $"Response status code {response.StatusCode}"));
            }
            catch (Exception e)
            {
                BigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }
    }
}
