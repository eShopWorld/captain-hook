using System.Collections.Generic;

namespace CaptainHook.Domain.Entities
{
    public class UriTransformEntity
    {
        /// <summary>
        /// A list of replacements
        /// </summary>
        public IDictionary<string, string> Replace { get; }

        public UriTransformEntity(IDictionary<string, string> replace)
        {
            Replace = replace;
        }
    }
}