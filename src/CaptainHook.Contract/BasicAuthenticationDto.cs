﻿namespace CaptainHook.Contract
{
    public class BasicAuthenticationDto : AuthenticationDto
    {
        public const string Type = "Basic";
        public string Username { get; set; }
        public string PasswordKeyName { get; set; }
        
        public BasicAuthenticationDto()
        {
            AuthenticationType = Type;
        }
    }
}
