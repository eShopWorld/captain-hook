﻿namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Local cache token for requests to whatever
    /// </summary>
    public class AuthToken
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
                ExpiresTime = DateTime.UtcNow.AddSeconds(_expiresIn);
            }
        }

        /// <summary>
        /// Wall clock time of expires in
        /// </summary>
        public DateTime ExpiresTime { get; private set; }

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
    }
}