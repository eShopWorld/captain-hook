using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CaptainHook.Common.Authentication
{
    [KnownType(typeof(OidcAuthenticationConfig))]
    [KnownType(typeof(BasicAuthenticationConfig))]
    public class AuthenticationConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public AuthenticationConfig()
        {
            Type = AuthenticationType.None;
        }

        /// <summary>
        /// String for now, enums and the like might be better
        /// </summary>
        [JsonProperty(Order = 1, DefaultValueHandling = DefaultValueHandling.Include)]
        public AuthenticationType Type { get; set; }
    }
}
