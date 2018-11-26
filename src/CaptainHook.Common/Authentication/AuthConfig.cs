namespace CaptainHook.Common.Authentication
{
    //todo generalise this and inject from config and keyvalut
    //todo this should be generic so can be used by any provider
    public class AuthConfig
    {
        /// <summary>
        /// //todo put this in ci authConfig/production authConfig
        /// </summary>
        public string Uri { get; set; } = "https://security-sts.ci.eshopworld.net";

        /// <summary>
        /// 
        /// </summary>
        public string ClientId { get; set; } = "tooling.eda.client";

        /// <summary>
        /// Gets it from keyvault
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Scopes { get; set; } = "eda.api.all";
    }
}