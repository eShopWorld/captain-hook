using System.Linq;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Extensions
{
    public static class JArrayExtensions
    {
        public static bool IsArrayOf(this JArray jArray, JTokenType typeToCheck)
        {
            return jArray.Values().All(x => x?.Type == typeToCheck);
        }

        public static string ToValuesString(this JArray jArray, char separator = ',')
        {
            var values = jArray.Values().OfType<JValue>().Select(v => v.Value.ToString());
            return string.Join(separator, values);
        }
    }
}