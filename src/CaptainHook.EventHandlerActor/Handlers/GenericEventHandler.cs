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
        public virtual Task Call<TRequest>(TRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
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

            var response = await _client.PostAsJsonReliability(WebHookConfig.Uri, data.Payload, BigBrother);

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