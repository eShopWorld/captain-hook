namespace CaptainHook.Contract
{
    public class NoAuthenticationDto : AuthenticationDto
    {
        public const string Type = "None";
        
        public NoAuthenticationDto()
        {
            AuthenticationType = Type;
        }
    }
}
