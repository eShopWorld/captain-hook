namespace CaptainHook.Domain.Entities
{
    public class BasicAuthenticationEntity : AuthenticationEntity
    {
        /// <summary>
        /// User name
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Password
        /// </summary>
        public string PasswordKeyName { get; }

        public BasicAuthenticationEntity(string username, string passwordKeyName)
        {
            Username = username;
            PasswordKeyName = passwordKeyName;
        }
    }
}
