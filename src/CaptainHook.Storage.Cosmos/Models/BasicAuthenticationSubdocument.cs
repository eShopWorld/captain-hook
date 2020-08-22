﻿namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Basic authentication model in cosmos db
    /// </summary>
    internal class BasicAuthenticationSubdocument : AuthenticationSubdocument
    {
        /// <summary>
        /// Username for basic authentication
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password for basic authentication
        /// </summary>
        public string Password { get; set; }
    }
}
