using CaptainHook.Common.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptainHook.Common.Nasty
{
    public static class ModelParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="payload"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        public static string ParsePayloadPropertyAsString(string name, object payload, JToken jObject = null)
        {
            if (jObject == null)
            {
                jObject = GetJObject(payload);
            }

            var value = jObject.SelectToken(name);

            if (value == null)
            {
                return null;
            }

            return value.Type == JTokenType.Object ? value.ToString(Formatting.None) : value.Value<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="sourcePayload"></param>
        /// <param name="destinationType"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        public static JToken ParsePayloadProperty(ParserLocation location, object sourcePayload, DataType destinationType, JToken jObject = null)
        {
            if (jObject == null)
            {
                jObject = GetJObject(sourcePayload);
            }

            return destinationType == DataType.String ? 
                ParsePayloadPropertyAsString(location.Path, sourcePayload) : 
                jObject.SelectToken(location.Path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public static JToken GetJObject(object payload, DataType destinationType = DataType.Model)
        {
            if (destinationType == DataType.String)
            {
                return payload as string;
            }

            if (payload is string payloadAsString)
            {
                return JObject.Parse(payloadAsString);
            }

            if (payload is int payloadAsInt)
            {
                return new JValue(payloadAsInt);
            }

            return null;
        }
    }
}
