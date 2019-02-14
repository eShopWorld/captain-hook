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

                var uri = RequestBuilder.BuildUri(WebhookConfig, messageData.Payload);
                
                var payload = RequestBuilder.BuildPayload(WebhookConfig, messageData.Payload, new Dictionary<string, string>());
                
                void TelemetryEvent(string msg)
                {
                    BigBrother.Publish(new HttpClientFailure(messageData.Handle, messageData.Type, messageData.Payload, msg));
                }

                var response = await _client.ExecuteAsJsonReliably(WebhookConfig.HttpVerb, uri, payload, TelemetryEvent);
                
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
