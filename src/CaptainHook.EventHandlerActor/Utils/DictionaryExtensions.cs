using System;
using System.Collections.Generic;
using System.Text;

namespace CaptainHook.EventHandlerActor.Utils
{
    public static class DictionaryExtensions
    {
        public static string ToDebugString(this Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
                return "null";

            if (dictionary.Count == 0)
                return "empty";

            var stringBuilder = new StringBuilder();
            foreach (var (key, value) in dictionary)
                stringBuilder.Append($"{key}: {value}{Environment.NewLine}");
            return stringBuilder.ToString();
        }
    }
}