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
        public AuthenticationType Type { get; set; }
    }
}
