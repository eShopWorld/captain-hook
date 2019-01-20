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
        public static string ParseDomainType(string payload, JObject jObject = null)
        {
            if (jObject == null)
            {
                jObject = JObject.Parse(payload);
            }

            var domainType = ((JProperty)jObject.Parent).Name;
            return domainType;
        }

        public static Guid ParseOrderCode(string payload, JObject jObject = null)
        {
            if (jObject == null)
            {
                jObject = JObject.Parse(payload);
            }

            var orderCode = jObject.SelectToken("OrderCode").Value<string>();
            if (Guid.TryParse(orderCode, out var result))
            {
                return result;
            }

            throw new FormatException($"cannot parse order code in payload {orderCode}");
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
                jObject = JObject.Parse(payload);
            }

            var innerPayload = jObject.SelectToken(dtoName).ToString(Formatting.None);
            if (innerPayload != null)
            {
                return innerPayload;
            }
            throw new FormatException($"cannot parse order to get the request dto {payload}");
        }

        public static string ParseBrandType(string payload, JObject jObject = null)
        {
            if (jObject == null)
            {
                jObject = JObject.Parse(payload);
            }
            var brandType = jObject.SelectToken("BrandCode").Value<string>();
            return brandType;
        }

        public static (string brandType, string domainType) ParseBrandAndEventType(MessageData data)
        {
            var jObject = JObject.Parse(data.Payload);

            var brandType = ParseBrandType(data.Payload, jObject);

            //todo need to something here as the type is not in the payload so need to get it from 
            var domainType = data.Type;

            return (brandType, domainType);
        }
    }
}