using System;
using Newtonsoft.Json;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    /// <summary>
    /// Local cache token for requests to whatever
    /// </summary>
    public class OidcAuthenticationToken 
    {
        private int _expiresIn;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Time in seconds
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn
        {
            get => _expiresIn;
            set
            {
                _expiresIn = value;
                ExpireTime = DateTime.UtcNow.AddSeconds(_expiresIn);
            }
        }

        /// <summary>
        /// Wall clock time of expires in
        /// </summary>
        public DateTime ExpireTime { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// The time the token was refreshed at
        /// </summary>
        public DateTime TimeOfRefresh { get; set; }
    }
}
