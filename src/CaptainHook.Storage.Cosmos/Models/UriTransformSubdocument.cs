using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace CaptainHook.Storage.Cosmos.Models
{
    public class UriTransformSubdocument
    {
        public UriTransformSubdocument(IDictionary<string, string> replace)
        {
            Replace = new ReadOnlyDictionary<string, string>(replace ?? new Dictionary<string, string>());
        }

        [JsonProperty("replace")]
        public IDictionary<string, string> Replace { get; }
    }
}