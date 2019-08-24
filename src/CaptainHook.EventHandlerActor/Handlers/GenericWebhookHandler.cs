using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IHttpClientBuilder
    {
        Task<HttpClient> BuildAsync(
            WebhookConfig config,
            AuthenticationType authenticationScheme,
            string correlationId,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Builds out a http client for the given request flow
    /// </summary>
    public class HttpClientBuilder : IHttpClientBuilder
    {
        private readonly IAuthenticationHandlerFactory _authenticationHandlerFactory;
        private readonly IIndex<string, HttpClient> _httpClients;

        public HttpClientBuilder(
            IAuthenticationHandlerFactory authenticationHandlerFactory, 
            IIndex<string, HttpClient> httpClients)
        {
            _authenticationHandlerFactory = authenticationHandlerFactory;
            _httpClients = httpClients;
        }

        /// <summary>
        /// Gets a configured http client for use in a request from the http client factory
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="config"></param>
        /// <param name="authenticationScheme"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public async Task<HttpClient> BuildAsync(WebhookConfig config, AuthenticationType authenticationScheme, string correlationId, CancellationToken cancellationToken)
        {
            var uri = new Uri(config.Uri);

            if (!_httpClients.TryGetValue(uri.Host, out var httpClient))
            {
                throw new ArgumentNullException(nameof(httpClient), $"HttpClient for {uri.Host} was not found");
            }

            if (authenticationScheme == AuthenticationType.None)
            {
                return httpClient;
            }

            var acquireTokenHandler = await _authenticationHandlerFactory.GetAsync(uri, cancellationToken);
            await acquireTokenHandler.GetTokenAsync(httpClient, cancellationToken);

            httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

            return httpClient;
        }
    }

    public interface IRequestLogger
    {
        Task LogAsync(
            HttpClient httpClient,
            HttpResponseMessage response,
            MessageData messageData,
            Uri uri,
            HttpVerb httpVerb
            );
    }

    /// <summary>
    /// Handles logging successful or failed webhook calls to the destination endpoints
    /// Extends the telemetry but emitting all request and response properties in the failure flows
    /// </summary>
    public class RequestLogger : IRequestLogger
    {
        private readonly IBigBrother _bigBrother;

        public RequestLogger(IBigBrother bigBrother)
        {
            _bigBrother = bigBrother;
        }

        public async Task LogAsync(
            HttpClient httpClient,
            HttpResponseMessage response,
            MessageData messageData,
            Uri uri,
            HttpVerb httpVerb
            )
        {
            _bigBrother.Publish(new WebhookEvent(
                messageData.Handle,
                messageData.Type,
                $"Response status code {response.StatusCode}",
                uri.AbsoluteUri,
                httpVerb,
                response.StatusCode,
                messageData.CorrelationId));

            //only log the failed requests in more depth
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            _bigBrother.Publish(new FailedWebHookEvent(
                httpClient.DefaultRequestHeaders.ToString(),
                response.Headers.ToString(),
                messageData.Payload ?? string.Empty,
                await GetPayloadAsync(response),
                messageData.Handle,
                messageData.Type,
                $"Response status code {response.StatusCode}",
                uri.AbsoluteUri,
                httpVerb,
                response.StatusCode,
                messageData.CorrelationId));
        }

        private static async Task<string> GetPayloadAsync(HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return string.Empty;
            }
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }

    /// <summary>
    /// Generic WebHookConfig Handler which executes the call to a webhook based on the supplied configuration
    /// </summary>
    public class GenericWebhookHandler : IHandler
    {
        protected readonly IBigBrother BigBrother;
        protected readonly IHttpClientBuilder HttpClientBuilder;
        protected readonly IRequestBuilder RequestBuilder;
        protected readonly IRequestLogger RequestLogger;
        protected readonly WebhookConfig WebhookConfig;

        public GenericWebhookHandler(
            IHttpClientBuilder httpClientBuilder,
            IRequestBuilder requestBuilder,
            IRequestLogger requestLogger,
            IBigBrother bigBrother,
            WebhookConfig webhookConfig)
        {
            BigBrother = bigBrother;
            RequestBuilder = requestBuilder;
            RequestLogger = requestLogger;
            WebhookConfig = webhookConfig;
            HttpClientBuilder = httpClientBuilder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="metadata"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task CallAsync<TRequest>(TRequest request, IDictionary<string, object> metadata, CancellationToken cancellationToken)
        {
            try
            {
                if (!(request is MessageData messageData))
                {
                    throw new Exception("injected wrong implementation");
                }

                var uri = RequestBuilder.BuildUri(WebhookConfig, messageData.Payload);
                var httpVerb = RequestBuilder.SelectHttpVerb(WebhookConfig, messageData.Payload);
                var payload = RequestBuilder.BuildPayload(this.WebhookConfig, messageData.Payload, metadata);
                var authenticationScheme = RequestBuilder.SelectAuthenticationScheme(WebhookConfig, messageData.Payload);
                var config = RequestBuilder.SelectWebhookConfig(WebhookConfig, messageData.Payload);

                var httpClient = await HttpClientBuilder.BuildAsync(config, authenticationScheme, messageData.CorrelationId, cancellationToken);

                //don't want to inject bb into every class for the sake of it, passing it around here
                var handler = new HttpFailureLogger(BigBrother, messageData, uri.AbsoluteUri, httpVerb);
                var response = await httpClient.ExecuteAsJsonReliably(httpVerb, uri, payload, handler, token: cancellationToken);

                await RequestLogger.LogAsync(httpClient, response, messageData, uri, httpVerb);
            }
            catch (Exception e)
            {
                BigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }
    }
}
