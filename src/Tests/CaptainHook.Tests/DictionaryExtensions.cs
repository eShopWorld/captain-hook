using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptainHook.Tests
{
    public static class DictionaryExtensions
    {
        public static string ToJson(this Dictionary<string, string> dictionary, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(dictionary, formatting);
        }
    }
}
