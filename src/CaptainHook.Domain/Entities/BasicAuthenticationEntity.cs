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
        public string Password { get; }

        public BasicAuthenticationEntity(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
