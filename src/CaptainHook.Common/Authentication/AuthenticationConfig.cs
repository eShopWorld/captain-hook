using System;

namespace CaptainHook.Common.Authentication
{
    public class AuthenticationConfig
    {
        public AuthenticationConfig()
        {
            AuthenticationType = "none";
        }

        /// <summary>
        /// String for now, enums and the like might be better
        /// </summary>
        public string AuthenticationType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AuthenticationType AuthenticationTypeAsEnum
        {
            get
            {
                if (string.Equals("none", AuthenticationType, StringComparison.OrdinalIgnoreCase))
                {
                    return Authentication.AuthenticationType.None;
                }

                if (string.Equals("basic", AuthenticationType, StringComparison.OrdinalIgnoreCase))
                {
                    return Authentication.AuthenticationType.Basic;
                }

                if (string.Equals("oauth", AuthenticationType, StringComparison.OrdinalIgnoreCase))
                {
                    return Authentication.AuthenticationType.OAuth;
                }

                if (string.Equals("custom", AuthenticationType, StringComparison.OrdinalIgnoreCase))
                {
                    return Authentication.AuthenticationType.Custom;
                }

                throw new Exception("authentication scheme is not set");
            }
        }
    }
}
