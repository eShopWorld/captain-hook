using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    /// <summary>
    /// Basic Authentication Handler which returns a http client with a basic http authentication header
    /// </summary>
    public class BasicAuthenticationHandler : AuthenticationHandler, IAuthenticationHandler
    {
        protected readonly BasicAuthenticationConfig BasicAuthenticationConfig;

        public BasicAuthenticationHandler(AuthenticationConfig authenticationConfig)
        {
            var basicAuthenticationConfig = authenticationConfig as BasicAuthenticationConfig;

            BasicAuthenticationConfig = basicAuthenticationConfig ?? throw new ArgumentException($"configuration for basic authentication is not of type {typeof(BasicAuthenticationConfig)}", nameof(authenticationConfig));
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets a token and updates the http client with the authentication header
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            var encodedValue = CreateBasicAuthToken(BasicAuthenticationConfig.Username, BasicAuthenticationConfig.Password);
            var token = $"Basic {encodedValue}";

            return await Task.FromResult(token);
        }

        public static string CreateBasicAuthToken(string username, string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        }

        public bool HasConfigChanged(AuthenticationConfig newConfig)
        {
            var hasChanged = true;
            if (newConfig is BasicAuthenticationConfig authConfig)
            {
                if (authConfig.Username == BasicAuthenticationConfig.Username && authConfig.Password == BasicAuthenticationConfig.Password)
                {
                    hasChanged = false;
                }
            }
            return hasChanged;
        }
    }
}
