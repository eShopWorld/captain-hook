namespace CaptainHook.Common.Authentication
{
    using System;

    /// <summary>
    /// Local cache token for requests to whatever
    /// </summary>
    public class AuthToken
    {
        /// <summary>
        /// 
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Time in seconds
        /// </summary>
        public int ExpiresIn { get; private set; }

        /// <summary>
        /// Wall clock time of expires in
        /// </summary>
        public DateTime ExpiresTime { get; private set; }

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