using System.Collections.Generic;

namespace CaptainHook.Contract
{
    /// <summary>
    /// Defines a webhook URI transform
    /// </summary>
    public class UriTransformDto
    {
        /// <summary>
        /// A list of replacements
        /// </summary>
        public IDictionary<string, string> Replace { get; set; }
    }
}
