using System.Collections.Generic;

namespace CaptainHook.Api
{
    /// <summary>
    /// The configuration settings for this service
    /// 
    /// NOTE - consider moving this to DevOps
    /// </summary>
    public class ServiceConfigurationOptions
    {
        /// <summary>
        /// The scopes to assert on the Api
        /// </summary>
        public List<string> RequiredScopes { get; set; }

        /// <summary>
        /// The Authority base address
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// The Api name
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// The Api secret
        /// </summary>
        public string ApiSecret { get; set; }     

        /// <summary>
        /// Indicates if the service should require https
        /// </summary>
        public bool IsHttps => !string.IsNullOrWhiteSpace(Authority) && Authority.StartsWith("https");
    }
}
