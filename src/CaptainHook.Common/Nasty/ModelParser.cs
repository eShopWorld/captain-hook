using System;
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
        [Obsolete]
        public static Guid ParsePayloadPropertyAsGuid(string name, string payload, JObject jObject = null)
        {
            if (jObject == null)
            {
                jObject = GetJObject(payload);
            }

            var orderCode = jObject.SelectToken(name).Value<string>();
            if (Guid.TryParse(orderCode, out var result))
            {
                return result;
            }

            throw new FormatException($"cannot parse order code in payload {orderCode}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="sourcePayload"></param>
        /// <returns></returns>
        public static string ParsePayloadPropertyAsString(ParserLocation rule, string sourcePayload)
        {
            var value = ParsePayloadProperty(rule, sourcePayload);

            return value.ToString(Formatting.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="payload"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        public static string ParsePayloadPropertyAsString(string name, string payload, JObject jObject = null)
        {
            if (jObject == null)
            {
                jObject = GetJObject(payload);
            }

            var value = jObject.SelectToken(name).Value<string>();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            throw new FormatException($"cannot parse order code in payload {value}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="sourcePayload"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        public static JToken ParsePayloadProperty(ParserLocation location, string sourcePayload, JObject jObject = null)
        {
            if (jObject == null)
            {
                jObject = GetJObject(sourcePayload);
            }

            var value = jObject.SelectToken(location.Path);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="value"></param>
        /// <param name="container"></param>
        public static void AddPropertyToPayload(ParserLocation location, JToken value, JContainer container)
        {
            if (value == null)
            {
                return;
            }

            container.Add(new JProperty(location.Path, value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static JObject GetJObject(string payload)
        {
            return string.IsNullOrWhiteSpace(payload) ? new JObject() : JObject.Parse(payload);
        }

        /// <summary>
        /// Converts a string to a JValue
        /// Should use it when it's a primitive type and what to convert into a JValue
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static JValue GetJValue(string payload)
        {
            return new JValue(payload);
        }
    }
}
