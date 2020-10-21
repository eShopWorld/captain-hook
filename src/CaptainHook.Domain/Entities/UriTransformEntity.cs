using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
            Replace = new ReadOnlyDictionary<string, string>(replace ?? 
                new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase));
        }
    }
}