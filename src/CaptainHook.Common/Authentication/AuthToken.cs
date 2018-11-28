namespace CaptainHook.Common.Authentication
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Local cache token for requests to whatever
    /// </summary>
    public class AuthToken
    {
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
        public int ExpiresIn { get; private set; }

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

        /// <summary>
        /// Updates the local expiration time in seconds and gives an estimated expires time
        /// </summary>
        /// <param name="expiresIn">Expires in seconds after creation time</param>
        public void Update(int expiresIn)
        {
            ExpiresIn = expiresIn;
            ExpiresTime = DateTime.UtcNow.AddSeconds(expiresIn);
        }
    }
}