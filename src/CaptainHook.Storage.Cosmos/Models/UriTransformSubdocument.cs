using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CaptainHook.Storage.Cosmos.Models
{
    public class UriTransformSubdocument
    {
        public UriTransformSubdocument(IDictionary<string, string> replace)
        {
            Replace = new ReadOnlyDictionary<string, string>(replace ?? new Dictionary<string, string>());
        }

        public IDictionary<string, string> Replace { get; }
    }
}