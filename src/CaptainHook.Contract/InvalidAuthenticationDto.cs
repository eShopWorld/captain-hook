namespace CaptainHook.Contract
{
    public class InvalidAuthenticationDto : AuthenticationDto
    {
        public const string Type = "Invalid";
        
        public InvalidAuthenticationDto()
        {
            AuthenticationType = Type;
        }
    }
}
