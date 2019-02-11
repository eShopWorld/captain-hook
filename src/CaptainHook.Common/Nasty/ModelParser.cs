namespace CaptainHook.Common.Nasty
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// todo nuke this in V1
    /// </summary>
    public static class ModelParser
    {
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

        public static void AddPropertyToPayload(string name, string value, JObject jObject)
        {
            jObject.Add(name, new JValue(value));
        }

        /// <summary>
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="dtoName"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        public static string GetInnerPayload(string payload, string dtoName, JObject jObject = null)
        {
            if (jObject == null)
            {
                jObject = GetJObject(payload);
            }

            var innerPayload = jObject.SelectToken(dtoName).ToString(Formatting.None);
            if (innerPayload != null)
            {
                return innerPayload;
            }
            throw new FormatException($"cannot parse order to get the request dto {payload}");
        }

        public static JObject GetJObject(string payload)
        {
            return JObject.Parse(payload);
        }
    }
}
