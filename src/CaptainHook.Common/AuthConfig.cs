namespace CaptainHook.Common
{
    //todo generalise this and inject from config and keyvalut
    //todo this should be generic so can be used by any provider
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