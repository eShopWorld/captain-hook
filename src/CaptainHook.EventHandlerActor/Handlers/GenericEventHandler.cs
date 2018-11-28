namespace CaptainHook.EventHandlerActor.Handlers
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Authentication;
    using Common.Nasty;
    using Common.Telemetry;
    using Eshopworld.Core;

    public class GenericEventHandler : IHandler
    {
        private readonly HttpClient _client;
        private readonly IBigBrother _bigBrother;

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
            _bigBrother = bigBrother;
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

            //make a call to their api
            //todo polly 429 and 503
            var response = await _client.PostAsync(WebHookConfig.Uri, new StringContent(data.Payload, Encoding.UTF8, "application/json"));

            //todo decisions, either continue to make calls or create a new event and throw it on to the appropriate topic for process
            if (response.IsSuccessStatusCode)
            {
                //todo don't be sending customer payloads around for logging
                _bigBrother.Publish(new WebhookExecuted(data.Handle, data.Type, data.Payload));
            }
            else
            {
                //todo webhook failure
            }

            var dto = new HttpResponseDto
            {
                Content = await response.Content.ReadAsStringAsync(),
                StatusCode = (int)response.StatusCode
            };

            return dto;
        }
    }
}