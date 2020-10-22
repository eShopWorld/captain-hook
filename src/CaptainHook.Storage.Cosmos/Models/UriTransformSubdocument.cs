using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CaptainHook.Storage.Cosmos.Models
{
    public class UriTransformSubdocument
    {
        private static readonly Dictionary<string, string> EmptyReplacementsDictionary = new Dictionary<string, string>();

        public UriTransformSubdocument(IDictionary<string, string> replace)
        {
            Replace = new ReadOnlyDictionary<string, string>(replace ?? EmptyReplacementsDictionary);
        }
        
        public IDictionary<string, string> Replace { get; }
    }
}