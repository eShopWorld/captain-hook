namespace CaptainHook.Contract
{
    public class BasicAuthenticationDto : AuthenticationDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
