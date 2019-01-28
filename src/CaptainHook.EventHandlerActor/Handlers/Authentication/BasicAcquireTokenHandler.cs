using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    /// <summary>
    /// Basic Authentication Handler which returns a http client with a basic http authentication header
    /// </summary>
    public class BasicAcquireTokenHandler : AuthenticationHandler, IAcquireTokenHandler
    {
        protected readonly BasicAuthenticationConfig BasicAuthenticationConfig;

        public BasicAcquireTokenHandler(AuthenticationConfig authenticationConfig)
        {
            var basicAuthenticationConfig = authenticationConfig as BasicAuthenticationConfig;

            BasicAuthenticationConfig = basicAuthenticationConfig ?? throw new ArgumentException($"configuration for basic authentication is not of type {typeof(BasicAuthenticationConfig)}", nameof(authenticationConfig));
        }

        /// <summary>
        /// Gets a token and updates the http client with the authentication header
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public virtual async Task GetToken(HttpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Http Client is null");
            }

            client.SetBasicAuthentication(BasicAuthenticationConfig.Username, BasicAuthenticationConfig.Password);

            await Task.CompletedTask;
        }
    }
}
