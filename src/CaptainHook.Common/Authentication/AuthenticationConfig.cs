using Newtonsoft.Json;

namespace CaptainHook.Common.Authentication
{
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
        [JsonProperty(Order = 1)]
        public AuthenticationType Type { get; set; }
    }
}
