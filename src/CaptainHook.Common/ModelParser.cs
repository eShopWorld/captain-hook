using System;
using CaptainHook.Common.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptainHook.Common
{
    public static class ModelParser
    {

        /// <summary>
        /// Using a jpath query parses a jobject to get it's type.
        /// </summary>
        /// <param name="jpath">Query value</param>
        /// <param name="payload">The payload which will be parsed if jobject is not supplied</param>
        /// <param name="jObject">The parses Jobject to use if one is supplied</param>
        /// <returns></returns>
        public static string ParsePayloadPropertyAsString(string jpath, object payload, JToken jObject = null)
        {
            if (jObject == null)
            {
                jObject = GetJObject(payload);
            }

            var value = jObject.SelectToken(jpath);

            if (value == null)
            {
                throw new ArgumentNullException(nameof(jpath), $"was not found in the model payload with a value of {jpath}. ResponsePayload {payload}");
            }

            return value.Type == JTokenType.Object ? value.ToString(Formatting.None) : value.Value<string>();
        }

        /// <summary>
        /// Parses the supplied source payload with the supplied ParserLocation.Path and returns the value as a JToken so it can be added to a JObject. 
        /// Will use a supplied Jobject instead if one is supplied.
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
        /// Simple parser location to return proper jtoken based on the datatype requested and datatype required output.
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

            throw new ArgumentException($"ResponsePayload could not be parsed as string or int {payload}");
        }
    }
}
