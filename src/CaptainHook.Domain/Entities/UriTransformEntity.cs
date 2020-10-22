using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CaptainHook.Domain.Entities
{
    public class UriTransformEntity
    {
        private static readonly Dictionary<string, string> EmptyReplacementsDictionary = new Dictionary<string, string>();

        /// <summary>
        /// A list of replacements
        /// </summary>
        public IReadOnlyDictionary<string, string> Replace { get; }

        public UriTransformEntity(IDictionary<string, string> replace)
        {
            var caseInsensitiveDictionary = new Dictionary<string, string>(
                replace ?? EmptyReplacementsDictionary,
                StringComparer.OrdinalIgnoreCase);
            Replace = new ReadOnlyDictionary<string, string>(caseInsensitiveDictionary);
        }
    }
}