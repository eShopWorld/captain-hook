using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Eshopworld.Platform.Messages;
using Eshopworld.Platform.Messages.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptainHook.EventHandlerActor.Handlers
{
    /// <summary>
    /// Generic WebHookConfig Handler which executes the call to a webhook based on the supplied configuration
    /// </summary>
    public class GenericWebhookHandler : IHandler
    {
        protected readonly IBigBrother BigBrother;
        protected readonly IHttpSender HttpSender;
        protected readonly IRequestBuilder RequestBuilder;
        protected readonly WebhookConfig WebhookConfig;
        private readonly IRequestLogger _requestLogger;
        private readonly IAuthenticationHandlerFactory _authenticationHandlerFactory;

        public GenericWebhookHandler(
            IHttpSender httpSender,
            IAuthenticationHandlerFactory authenticationHandlerFactory,
            IRequestBuilder requestBuilder,
            IRequestLogger requestLogger,
            IBigBrother bigBrother,
            WebhookConfig webhookConfig)
        {
            BigBrother = bigBrother;
            RequestBuilder = requestBuilder;
            _requestLogger = requestLogger;
            WebhookConfig = webhookConfig;
            _authenticationHandlerFactory = authenticationHandlerFactory;
            HttpSender = httpSender;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="metadata"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<bool> CallAsync<TRequest>(TRequest request, IDictionary<string, object> metadata, CancellationToken cancellationToken)
        {
            Uri uri = null;
            try
            {
                if (!(request is MessageData messageData))
                {
                    throw new InvalidOperationException("injected wrong implementation");
                }

                //todo refactor into a single call and a dto
                uri = RequestBuilder.BuildUri(WebhookConfig, messageData.Payload);
                if (uri == null)
                {
                    return true; //consider successful delivery
                }
                var httpMethod = RequestBuilder.SelectHttpMethod(WebhookConfig, messageData.Payload);
                var originalPayload = RequestBuilder.BuildPayload(WebhookConfig, messageData.Payload, metadata);
                var payload = WebhookConfig.PayloadTransformation == PayloadContractTypeEnum.WrapperContract
                    ? WrapPayload(originalPayload, WebhookConfig, messageData)
                    : originalPayload;

                var config = RequestBuilder.SelectWebhookConfig(WebhookConfig, messageData.Payload);
                var headers = RequestBuilder.GetHttpHeaders(WebhookConfig, messageData);
                var authenticationConfig = RequestBuilder.GetAuthenticationConfig(WebhookConfig, messageData.Payload);

                await AddAuthenticationHeaderAsync(cancellationToken, authenticationConfig, headers);

                var response = await HttpSender.SendAsync(
                     new SendRequest(httpMethod, uri, headers, payload, config.RetrySleepDurations, config.Timeout),
                     cancellationToken);

                await _requestLogger.LogAsync(response, messageData, payload, uri, httpMethod, headers, config);

                return !response.IsDeliveryFailure();
            }
            catch (Exception e)
            {
                BigBrother.Publish(new HttpSendFailureEvent(uri?.AbsoluteUri, e));
                throw;
            }
        }

        private string WrapPayload(string originalPayload, WebhookConfig webhookConfig, MessageData messageData)
        {
            var source = new NewtonsoftDeliveryStatusMessage
            {
                CallbackType = messageData.IsDlq ? CallbackTypeEnum.DeliveryFailure : CallbackTypeEnum.Callback,
                EventType = webhookConfig.EventType,
                MessageId = messageData.ServiceBusMessageId,
                StatusCode = null, //this is never specified for non callback
                Payload = JObject.Parse(originalPayload),
                TelemetryUri = null //for now
            };

            using (var sw = new StringWriter())
            {
                using (var writer = new JsonTextWriter(sw))
                {
                    JsonSerializer.CreateDefault().Serialize(writer, source);
                    writer.Flush();
                    return sw.ToString();
                }
            }
        }

        protected async Task AddAuthenticationHeaderAsync(CancellationToken cancellationToken, WebhookConfig config, WebHookHeaders webHookHeaders)
        {
            if (config.AuthenticationConfig.Type != AuthenticationType.None)
            {
                var acquireTokenHandler = await _authenticationHandlerFactory.GetAsync(config, cancellationToken);
                var result = await acquireTokenHandler.GetTokenAsync(cancellationToken);
                webHookHeaders.AddRequestHeader(Constants.Headers.Authorization, result);
            }
        }
    }
}
