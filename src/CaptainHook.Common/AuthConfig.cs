namespace CaptainHook.Common
{
    public class AuthConfig
    {
        /// <summary>
        /// //todo put this in ci authConfig/production authConfig
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets it from keyvault
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Scopes { get; set; }
    }
}